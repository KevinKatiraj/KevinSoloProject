using System.ComponentModel.DataAnnotations;
namespace KevinSoloProject.Models;
#pragma warning disable CS8618
using System.Web;
using System.ComponentModel;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

public class Favourite
{
    [Key]
    public int FavouriteId { get; set; }
    public int UserId { get; set; }
    public int ImageId { get; set; }
    public Image? FavouriteClothes { get; set; }
    public User? User{get;set;}

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}