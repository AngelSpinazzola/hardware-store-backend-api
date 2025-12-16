using HardwareStore.Domain.Enums;
﻿using System.ComponentModel.DataAnnotations;

namespace HardwareStore.Application.Products
{
    public class UpdateProductImagesOrderDto
    {
        [Required]
        public List<UpdateProductImageOrderDto> Images { get; set; } = new();
    }
}
