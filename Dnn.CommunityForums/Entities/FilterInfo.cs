﻿//
// Community Forums
// Copyright (c) 2013-2024
// by DNN Community
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
//
//
using DotNetNuke.ComponentModel.DataAnnotations;
using System.Web.Caching;

namespace DotNetNuke.Modules.ActiveForums.Entities
{
    [TableName("activeforums_Filters")]
    [PrimaryKey("FilterId", AutoIncrement = true)]
    [Scope("ModuleId")]
    [Cacheable("activeforums_Filters", CacheItemPriority.Low)]
    public partial class FilterInfo
    {
        public int FilterId { get; set; }
        public string Find { get; set; }
        public string Replace { get; set; }
        public string FilterType { get; set; }
        public int PortalId { get; set; }
        public int ModuleId { get; set; }
    }
}

