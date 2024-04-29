using System.ComponentModel;
namespace DataImport.Models
{
    public enum AgentActionsFile
    {
        [Description("Always delete")]
        AlwaysDelete = 0,
        [Description("Delete on successful")]
        DeleteOnSuccessful = 1,
        [Description("Never delete")]
        NeverDelete = 2
    }
}
