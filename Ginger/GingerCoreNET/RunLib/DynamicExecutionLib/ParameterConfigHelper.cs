﻿using Ginger.ExecuterService.Contracts.V1.ExecutionConfiguration;
using GingerCore.Variables;
using System;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicExecutionLib
{
    public static class ParameterConfigHelper
    {

        public static void ValidateParameterConfig(ParameterConfig Parameter)
        {
            if(Parameter == null || string.IsNullOrEmpty(Parameter.Value) || string.IsNullOrEmpty(Parameter.Name))
            {
                throw new ArgumentException($"{nameof(Parameter)} Name or Value cannot be null or empty");
            }
        }

        public static VariableBase CreateParameterFromConfig(ParameterConfig Parameter)
        {
            ValidateParameterConfig(Parameter);
            return new VariableDynamic()
            {
                ValueExpression = Parameter.Value,
                Name = Parameter.Name
            };
        }
        public static void UpdateParameterFromConfig(ParameterConfig ParameterConfig, ref VariableBase ParameterFromGinger)
        {
            ValidateParameterConfig(ParameterConfig);

            ParameterFromGinger.Name = ParameterConfig.Name;
            ParameterFromGinger.SetValue(ParameterConfig.Value);
        }
    }
}