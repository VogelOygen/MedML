using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedML.Models
{
    [Table("heart_disease_records")]
    public class HeartDiseaseRecord
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("age")]
        public int Age { get; set; }

        [Column("sex")]
        public string Sex { get; set; }

        [Column("chest_pain_type")]
        public string ChestPainType { get; set; }

        [Column("resting_bp")]
        public int RestingBP { get; set; }

        [Column("cholesterol")]
        public int Cholesterol { get; set; }

        [Column("fasting_bs")]
        public int FastingBS { get; set; }

        [Column("resting_ecg")]
        public string RestingECG { get; set; }

        [Column("max_hr")]
        public int MaxHR { get; set; }

        [Column("exercise_angina")]
        public string ExerciseAngina { get; set; }

        [Column("oldpeak")]
        public float Oldpeak { get; set; }

        [Column("st_slope")]
        public string ST_Slope { get; set; }

        [Column("heart_disease")]
        public int HeartDisease { get; set; }
    }
}
