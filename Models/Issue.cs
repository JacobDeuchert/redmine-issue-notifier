using System;
using System.Collections.Generic;

namespace redmine_notifier.Models
{
    public class Issue
    {
        public long id { get; set; }
        public Field project { get; set; }
        public Field tracker { get; set; }
        public Field status { get; set; }
        public Field priority { get; set; }
        public Field author { get; set; }
        public string subject { get; set; }
        public string description { get; set; }
        public string start_date { get; set; }
        public int done_ratio { get; set; }
        public bool is_private { get; set; }
        public double? estimated_hours { get; set; }
        public string created_on { get; set; }
        public string updated_on { get; set; }
        public string closed_on { get; set; }
    }

    public class IssueResponse 
    { 
        public List<Issue> issues { get; set; }
        public long total_count { get; set; }

        public long offset { get; set; }
        public long limit { get; set; }
    }
}