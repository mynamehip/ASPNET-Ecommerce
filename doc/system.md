# System Design - E-Commerce ASP.NET Core MVC

---

## Overview

This document summarizes the current system design, request flow, and business rules of the e-commerce application.

---

## Architecture

The application follows a layered architecture:

* Controller Layer -> handle HTTP requests
* Service Layer -> business logic
* Data Layer -> database access through Entity Framework Core

Main request flow:

Request -> Controller -> Service -> DbContext -> Database -> Response

Current storefront highlights:

* Homepage hero has been replaced by a managed slider
* Product pricing can include direct per-product discount
* Cart, checkout, wishlist, product detail, and order snapshot all use the same effective price source

---

## Authentication Flow

### Register

1. User submits register form
2. Validate input
3. Hash password
4. Save user to database
5. Assign role = User
6. Redirect to login

---

### Login

1. User submits login form
2. Validate input
3. Find user by email
4. Compare password
5. If valid, create authentication cookie
6. Redirect to home

---

### Authorization

* Admin -> full access
* Employee -> manage products and categories
* User -> shopping only

---

## Homepage Slider Flow

1. Admin manages slider items in the Admin area
2. Each `SliderItem` contains:
   * `Type`
   * `DisplayOrder`
   * `IsActive`
   * `BackgroundImagePath`
3. Banner slides may also contain:
   * title
   * description
   * content
   * primary button text and URL
   * secondary button text and URL
4. Regular slides may contain optional `ClickUrl`
5. `HomeController` loads public settings and active slider items
6. `Index.cshtml` renders items in display order
7. If a banner has no background image, the storefront renders a default background

### Homepage Section Toggle

Admin can show or hide these homepage blocks through `SystemSetting`:

* slider
* categories
* new products
* featured products
* discount products

The storefront renders only sections whose `ShowHomepage*` flag is enabled.

---

## Product Flow

### Create Product (Admin)

1. Admin submits product form
2. Validate product data
3. Upload product images
4. Validate direct discount fields:
   * `IsDiscountActive`
   * `DiscountPercentage`
5. Save product to database
6. Save image paths
7. Return success

---

### Get Products (User)

1. User accesses product list
2. System fetches:
   * products
   * categories
3. Apply:
   * filter
   * sorting
   * pagination
4. Price filtering and sorting use effective price
5. Return result to view

### Direct Product Discount

1. Product stores base `Price`
2. Admin can enable `IsDiscountActive`
3. Admin sets `DiscountPercentage`
4. `ProductPricing` computes:
   * `EffectivePrice`
   * `DiscountAmount`
   * `HasActiveDiscount`
5. The same effective price is reused across:
   * product list
   * product details
   * wishlist
   * cart
   * checkout
   * order snapshot

---

## Cart Flow

### Add to Cart

1. User clicks "Add to cart"
2. Check if product exists
3. Check stock
4. Calculate effective unit price using direct product discount
5. Add item to cart in session
6. Update quantity if the item already exists

---

### Update Cart

1. User updates quantity
2. Validate stock
3. Update cart
4. Recalculate total price using effective unit price

---

## Checkout Flow

1. User opens checkout page
2. Enter shipping information
3. Validate data
4. Server rebuilds pricing from cart, not from client input
5. Create order
6. Create order items using discounted cart `UnitPrice`
7. Reduce product stock
8. Clear cart
9. Try sending order confirmation email if SMTP settings are configured
10. Redirect to confirmation page

---

## Order Flow

### User

* View order history
* View order details
* Look up an order by order number

### Admin

* View all orders
* Update order status:
  * Pending
  * Processing
  * Completed
  * Cancelled

---

## Review Flow

1. User purchases product
2. User submits review
3. System verifies the purchase and publishes the review immediately
4. Admin can hide the review or reply from the admin area when needed
5. Approved reviews display on the product page together with any admin reply

---

## Settings Flow (Admin)

Admin can update:

* logo
* homepage section visibility
* storefront pricing settings
* support email
* promo code settings
* SMTP delivery settings for order emails

### Slider Management

Slider management is a separate admin module.

* Supports banner slides and regular slides
* `RegularSlide.ClickUrl` is optional
* `RegularSlide` requires background image
* `Banner` can use uploaded background or fallback background on storefront
* Each slide has `IsActive` for hide/show without deletion

---

## Data Relationships

* User -> Orders (1-n)
* Order -> OrderItems (1-n)
* Product -> Category (n-1)
* Product -> Reviews (1-n)
* Product -> WishlistItems (1-n)
* Storefront -> SliderItems (logical 1-n through system settings)

---

## Validation Rules

* Email must be unique
* Password must be hashed
* Product price > 0
* Quantity >= 0
* Direct discount is valid only when percentage is within allowed range
* Homepage renders only active slider items
* Regular slide background is required
* Regular slide click URL is optional
* Banner can omit background image because storefront has a default fallback

---
