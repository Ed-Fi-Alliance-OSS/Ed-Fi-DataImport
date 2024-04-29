using DataImport.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System;
using Humanizer;

namespace DataImport.Web.Features.Shared.SelectListProviders
{
    public class FilesActionSelectListProvider
    {
        public IList<SelectListItem> GetSelectListItems()
        {
            var enums = new List<SelectListItem>();
            foreach (var value in Enum.GetValues(typeof(AgentActionsFile)))
            {
                enums.Add(new SelectListItem
                {
                    Value = value.ToString(),
                    Text = ((AgentActionsFile) value).Humanize()
                });
            }
            return enums;
        }
    }
}
