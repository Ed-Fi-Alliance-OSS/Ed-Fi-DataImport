
using DataImport.Models;
using System;

namespace DataImport.Common.ExtensionMethods
{
    public static class AgentExtensions
    {
        public static AgentActionsFile GetActionFileCode(this Agent value)
        {
            if (value.ActionFileCode is null)
                return AgentActionsFile.DeleteOnSuccessful;
            return ((AgentActionsFile) Enum.Parse(typeof(AgentActionsFile), value.ActionFileCode));
        }
    }
}
