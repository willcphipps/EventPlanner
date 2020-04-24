using System;
using System.ComponentModel.DataAnnotations;

namespace BeltExam.Models {
    public class CalandarEvent {
        [Key]
        public int CalandarEventId { get; set; }
        public int UserId { get; set; }
        public int ActivityId { get; set; }
        public DojoActivity ActivityOnCal { get; set; }
        public User UserJoin { get; set; } 
    }
}