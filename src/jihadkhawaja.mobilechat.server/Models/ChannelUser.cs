﻿using System.ComponentModel.DataAnnotations;

namespace jihadkhawaja.mobilechat.server.Models
{
    public class ChannelUser
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid ChannelId { get; set; }
        [Required]
        public Guid UserId { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime? DateCreated { get; set; }
    }
}
