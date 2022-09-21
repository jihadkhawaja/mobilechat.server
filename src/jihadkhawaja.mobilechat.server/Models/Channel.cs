using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace jihadkhawaja.mobilechat.server.Models
{
    public class Channel
    {
        [Key]
        public Guid Id { get; set; }
        [NotMapped]
        public string? Title { get; set; }
        public DateTime? DateCreated { get; set; }
    }
}
