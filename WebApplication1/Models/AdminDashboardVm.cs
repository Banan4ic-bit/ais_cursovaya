using System;
using System.Collections.Generic;

namespace WebApplication1.Models
{
    public class AdminDashboardVm
    {
        // Быстрые метрики
        public int CoursesCount { get; set; }
        public int TeachersCount { get; set; }
        public int OrganizationsCount { get; set; }
        public int RequestsCount { get; set; }

        // Ближайшие запуски курсов (назначения) с заполненностью
        public List<AssignmentUtilRow> UpcomingAssignments { get; set; } = new();

        // Последние заявки
        public List<RecentRequestRow> RecentRequests { get; set; } = new();

        public class AssignmentUtilRow
        {
            public int AssignmentId { get; set; }
            public string CourseName { get; set; } = "";
            public string TeacherName { get; set; } = "";
            public DateOnly StartDate { get; set; }
            public DateOnly EndDate { get; set; }
            public int Capacity { get; set; }       // вместимость курса
            public int Enrolled { get; set; }       // суммарно заявлено людей
            public int Remaining { get; set; }      // осталось мест
        }

        public class RecentRequestRow
        {
            public int RequestId { get; set; }
            public string OrgName { get; set; } = "";
            public string CourseName { get; set; } = "";
            public string TeacherName { get; set; } = "";
            public DateOnly StartDate { get; set; }
            public DateOnly EndDate { get; set; }
            public int People { get; set; }
        }
    }
}
