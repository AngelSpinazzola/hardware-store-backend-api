using HardwareStore.Domain.Enums;
﻿using System.ComponentModel.DataAnnotations;

namespace HardwareStore.Application.Products
{
    public class SetMainImageDto
    {
        [Required]
        public int ImageId { get; set; }             
    }
}
