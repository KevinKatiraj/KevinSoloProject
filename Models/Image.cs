using System.ComponentModel.DataAnnotations;
namespace KevinSoloProject.Models;
#pragma warning disable CS8618
using System.Web;
using System.ComponentModel;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


public  class Image
{
    [Key]
    public int? ImageId { get; set; }
    public string Myimage { get; set; }
    [Required]
    public string Stina {get;set;}
    [Required]
    public string Category {get;set;}
     [Required]
    public string Type {get;set;}
    [Required]
    public int UserId { get; set; }
    // Navigation property for related User object
    public User? Creator { get; set; }

    [NotMapped]
    public IFormFile? Clothes { get; set;}

    List<Favourite>? Favourites = new List<Favourite>();

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}