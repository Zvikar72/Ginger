﻿using Ginger.AnalyzerLib;
using GingerCore.Environments;
using GingerCore;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using amdocs.ginger.GingerCoreNET;
using Microsoft.VisualStudio.Services.Common;
using System.Text.RegularExpressions;
namespace Amdocs.Ginger.CoreNET.AnalyzerLib
{
    public class AnalyzeEnvApplication : AnalyzerItemBase
    {
        public static void AnalyzeEnvAppInBusinessFlows(BusinessFlow businessFlow, List<AnalyzerItemBase> issues)
        {
            businessFlow
                .Activities
                .Select((activity) => activity.TargetApplication)
                .Distinct()
                .ForEach((targetApplication) =>
                {
                    CheckIfTargetApplicationExistInEnvironment(targetApplication, businessFlow, ref issues);
                });
        }
        private static void CheckIfTargetApplicationExistInEnvironment(string TargetApplication, BusinessFlow businessFlow, ref List<AnalyzerItemBase> issues)
        {
            if (!string.IsNullOrEmpty(TargetApplication))
            {
                ProjEnvironment currentEnvironment = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().FirstOrDefault((projEnvironment) => projEnvironment.Name.Equals(businessFlow.Environment));

                if(currentEnvironment == null)
                {
                    return;
                }


                EnvApplication application = currentEnvironment?.Applications?.FirstOrDefault((appName) => appName.Name.Equals(TargetApplication));


                if (application == null || string.IsNullOrEmpty(application.Name))
                {
                    string EnvApps = string.Join(";", currentEnvironment.Applications.Select(p => p.Name)?.ToList());

                    AnalyzeEnvApplication AB = new()
                    {
                        Description = $"{GingerDicser.GetTermResValue(eTermResKey.Activity)} {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} not found in Environment: {currentEnvironment.Name}",
                        UTDescription = "MissingApplicationInEnvironment",
                        Details = $"{GingerDicser.GetTermResValue(eTermResKey.Activity)}  {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} = '{TargetApplication}' while Environment app(s) is: '{EnvApps}'",
                        HowToFix = $"Open the Environment Configurations Page and add set correct {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)}",
                        CanAutoFix = eCanFix.No,
                        Status = eStatus.NeedFix,
                        IssueType = eType.Error,
                        ItemParent = currentEnvironment.Name,
                        ItemName = TargetApplication,
                        Impact = "Execution probably will fail due to missing input value.",
                        ItemClass = GingerDicser.GetTermResValue(eTermResKey.TargetApplication),
                        Severity = eSeverity.Critical,
                        Selected = true,
                        FixItHandler = null
                    };
                    issues.Add(AB);
                    return;
                }

                ApplicationPlatform? ExistingApplication = WorkSpace.Instance.Solution.ApplicationPlatforms
                    .FirstOrDefault(applicationPlatform => applicationPlatform.Guid.Equals(application.ParentGuid) || applicationPlatform.AppName.Equals(application.Name));

                if (ExistingApplication == null || ExistingApplication.Platform.Equals(ePlatformType.NA))
                {
                    AnalyzeEnvApplication AB = new()
                    {
                        Description = $"{GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} Platform cannot be 'NA'",
                        UTDescription = "MissingApplicationInTargetApplication",
                        Details = $"{GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} Platform cannot be 'NA'",
                        FixItHandler = null,
                        Status = eStatus.NeedFix,
                        IssueType = eType.Error,
                        ItemParent = currentEnvironment?.Name,
                        ItemName = TargetApplication,
                        Impact = "Execution probably will fail due to missing input value.",
                        ItemClass = GingerDicser.GetTermResValue(eTermResKey.TargetApplication),
                        Severity = eSeverity.High,
                        Selected = true,
                        CanAutoFix = eCanFix.No
                    };
                    issues.Add(AB);
                }
            }

        }

        private static void FixMissingEnvApp(object? sender, EventArgs e)
        {

            AnalyzeEnvApplication AnalyzeTargetApplication = sender as AnalyzeEnvApplication;

            if (AnalyzeTargetApplication == null)
            {
                return;
            }


            string ApplicationName = AnalyzeTargetApplication.ItemName;
            string EnvironmentName = AnalyzeTargetApplication.ItemParent;

            ProjEnvironment SelectedEnvironment = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().FirstOrDefault((proj) => proj.Name.Equals(EnvironmentName));
            ApplicationPlatform ApplicationPlatform = WorkSpace.Instance.Solution.ApplicationPlatforms.FirstOrDefault((appPlat) => appPlat.AppName.Equals(ApplicationName));

            if (SelectedEnvironment == null || ApplicationPlatform == null)
            {
                return;
            }

            SelectedEnvironment.StartDirtyTracking();
            SelectedEnvironment.AddApplications([ApplicationPlatform]);

            AnalyzeTargetApplication.Status = eStatus.Fixed;

        }

        private static readonly object lockObject = new();

        public static bool DoesEnvParamOrURLExistInValueExp(string ValueExp , string CurrentEnvironment)
        {

            if (string.IsNullOrEmpty(ValueExp))
            {
                return true;
            }



           MatchCollection envParamMatches = GingerCore.ValueExpression.rxEnvParamPattern.Matches(ValueExp);

            if( envParamMatches == null || envParamMatches.Count == 0)
            {
                return true;
            }

            ProjEnvironment CurrentProjEnv = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().FirstOrDefault((proj)=>proj.Name.Equals(CurrentEnvironment));


            if(CurrentProjEnv == null)
            {
                return true;
            }


            foreach (Match envParamMatch in envParamMatches)
            {
                string AppName = string.Empty, GlobalParamName = string.Empty;

                GingerCore.ValueExpression.GetEnvAppAndParam(envParamMatch.Value, ref AppName , ref GlobalParamName);

                if(string.IsNullOrEmpty(AppName) || string.IsNullOrEmpty(GlobalParamName))
                {
                    return false;
                }

                EnvApplication CurrentEnvApp = CurrentProjEnv.Applications.FirstOrDefault((app) => app.Name.Equals(AppName));

                lock (lockObject)
                {
                    CurrentEnvApp?.ConvertGeneralParamsToVariable();
                }

                bool doesParamExistInCurrEnv =  CurrentEnvApp?.Variables?.Any((Param)=>Param.Name.Equals(GlobalParamName)) ?? false;

                if (!doesParamExistInCurrEnv)
                {
                    return false;
                }
            }

            MatchCollection envURLMatches = GingerCore.ValueExpression.rxEnvUrlPattern.Matches(ValueExp);

            if (envURLMatches == null || envURLMatches.Count == 0)
            {
                return true;
            }

            foreach(Match envURLMatch in envURLMatches)
            {
                string AppName = string.Empty;
                GingerCore.ValueExpression.GetEnvAppFromEnvURL(envURLMatch.Value , ref AppName);


                if (string.IsNullOrEmpty(AppName))
                {
                    return false;
                }

                return CurrentProjEnv.Applications.Any((envApp) => envApp.Name.Equals(AppName));
            }

            return false;
        }
    }
}