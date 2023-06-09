﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace UniversityApi.Model;

public class Groups
{
    [Key]
    public int GroupId { get; set; }
    [Required]

    public int NumberGroup { get; set; }
    [Required]

    public int YearCome { get; set; }

    public int FacultiesId { get; set; }
    [JsonIgnore]

    public Faculties Faculties { get; set; }

    public List<Students> Students { get; set; } = new();


    public int? ProfessionsId { get; set; }
    [JsonIgnore]

    public Professions?  Professions{ get; set; }

}
