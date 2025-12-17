namespace YP_API.Models.Dtos
{
    using System.Collections.Generic;

    public class UpdateAllergiesDto
    {
        /// <summary>
        /// Список выбранных аллергенов.
        /// </summary>
        public List<string> Allergies { get; set; } = new List<string>();
    }
}