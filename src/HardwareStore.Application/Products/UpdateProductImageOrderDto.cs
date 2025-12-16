using HardwareStore.Domain.Enums;
﻿using System.ComponentModel.DataAnnotations;

namespace HardwareStore.Application.Products
{
    public class UpdateProductImageOrderDto
    {
        [Required]
        public int ImageId { get; set; }               

        [Required]
        [Range(0, int.MaxValue)]
        public int DisplayOrder { get; set; }
    }
}
