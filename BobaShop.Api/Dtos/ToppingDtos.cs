// -----------------------------------------------------------------------------
// File: ToppingDtos.cs
// Project: BobaShop.Api
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// Description:
//   Defines Data Transfer Objects (DTOs) for creating and updating Topping 
//   records. These DTOs are used by the ToppingsController to validate and 
//   transfer client input data between the API and MongoDB models.
//   Demonstrates use of data annotations for input validation.
// -----------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

namespace BobaShop.Api.Dtos
{
    // -------------------------------------------------------------------------
    // DTO: ToppingCreateDto
    // Purpose:
    //   Represents input data when creating a new topping.
    //   Includes validation attributes for server-side model binding.
    // Mapping: ICTPRG554 PE1.1 / ICTPRG556 PE2.1
    // -------------------------------------------------------------------------
    public class ToppingCreateDto
    {
        // Name of the topping
        // Must be at least 2 characters long.
        [Required, MinLength(2)]
        public string Name { get; set; } = default!;

        // Price of the topping in AUD.
        // Range validation ensures realistic positive values.
        [Range(0, 1000)]
        public decimal Price { get; set; } = 0.80m;

        // Indicates whether the topping is available for selection.
        public bool IsActive { get; set; } = true;
    }

    // -------------------------------------------------------------------------
    // DTO: ToppingUpdateDto
    // Purpose:
    //   Inherits from ToppingCreateDto.
    //   Used for updating existing topping records.
    //   Reuses the same validation rules.
    // -------------------------------------------------------------------------
    public class ToppingUpdateDto : ToppingCreateDto { }
}
