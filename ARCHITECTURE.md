# Kien Truc He Thong - ASPNET Ecommerce

Tai lieu nay mo ta lai toan bo kien truc du an ASP.NET Ecommerce dua tren ma nguon hien co. Muc tieu cua ban viet lai nay la khong chi liet ke file va chuc nang, ma con giai thich ro:

1. Du lieu duoc luu o dau.
2. Request di qua nhung lop nao.
3. Controller goi ham gi.
4. Service xu ly bien nao, kiem tra dieu kien nao.
5. Vi sao giao dien hien dung ket qua ma nguoi dung nhin thay.
6. Vi sao he thong chon cach trien khai do thay vi cach khac.

Noi dung ben duoi duoc viet theo nguyen tac:

1. Di tu tang nen tang den tang nghiep vu.
2. Sau do di vao tung module chuc nang.
3. Trong moi module, neu can se tach thanh:
   - muc dich
   - diem vao
   - du lieu vao / du lieu ra
   - service va bien lien quan
   - luong xu ly tung buoc
   - tai sao UI hien thi nhu vay
   - rang buoc nghiep vu va ly do thiet ke

## 1. Tong Quan Kien Truc

Day la mot ung dung thuong mai dien tu xay dung bang ASP.NET Core MVC, trong do co day du cac thanh phan:

1. Storefront cong khai cho khach hang:
   - xem trang chu
   - xem danh sach san pham
   - xem chi tiet san pham
   - them gio hang
   - them wishlist
   - dat hang
   - theo doi va quan ly don cua chinh minh

2. He thong xac thuc va ho so:
   - dang ky
   - dang nhap
   - dang xuat
   - quen mat khau
   - dat lai mat khau
   - cap nhat profile
   - doi mat khau

3. Khu vuc quan tri:
   - dashboard
   - quan ly category
   - quan ly product
   - quan ly order
   - moderation review
   - quan ly user va role
   - cau hinh he thong

4. Cac ha tang ho tro:
   - Entity Framework Core + SQL Server
   - ASP.NET Core Identity
   - Session
   - Localization
   - SignalR
   - Resource `.resx`

Ve mat kien truc, he thong duoc tach thanh 4 tang chinh:

1. Tang MVC / Presentation
   - cac `Controller`
   - cac `View`
   - `ViewModel`

2. Tang Business / Application Service
   - cac service trong thu muc `Services/`

3. Tang Persistence
   - `ApplicationDbContext`
   - migrations
   - entity model

4. Tang Cross-cutting / Infrastructure
   - auth
   - session
   - cookie
   - localization
   - realtime hub
   - file upload

Luong co ban cua mot request MVC trong du an nay:

1. Trinh duyet gui request.
2. Routing chon controller/action.
3. Controller nhan du lieu input qua route, query string hoac form.
4. Controller kiem tra `ModelState`, `Authorize`, claim, role.
5. Controller goi service tuong ung.
6. Service doc/ghi `ApplicationDbContext`, session, Identity, payment gateway hoac SignalR.
7. Service tra ket qua ve controller.
8. Controller chon `View`, `Redirect`, `Json`, `NotFound`, `Challenge`, `Unauthorized`.
9. Razor layout + view duoc render thanh HTML.

Neu viet gon thanh mot cau:

`Request -> Controller -> Service -> DbContext / Identity / Session / SignalR -> View/Redirect/JSON`

## 2. Cau Truc Thu Muc Va Vai Tro Tung Nhom File

## 2.1. Data Layer

### 2.1.1. `Data/ApplicationDbContext.cs`

Day la trung tam cua tang du lieu. File nay co 3 vai tro lon:

1. Dinh nghia cac bang nghiep vu duoi dang `DbSet`.
2. Cau hinh quan he, index, precision, enum conversion.
3. Ke thua `IdentityDbContext<ApplicationUser>` de ket hop du lieu nghiep vu voi he thong Identity.

DbSet chinh:

1. `Categories`
2. `Products`
3. `ProductImages`
4. `ProductReviews`
5. `WishlistItems`
6. `Orders`
7. `OrderItems`
8. `OrderStatusHistories`
9. `SystemSettings`
10. `SliderItems`

Tai sao `ApplicationDbContext` quan trong:

1. Moi service nghiep vu deu di qua day khi can doc/ghi DB.
2. Cac rang buoc nhu unique review, unique wishlist, unique order number deu duoc khoa o day.
3. Day la noi dam bao entity framework luu enum dang string thay vi dang so, giup DB de doc hon.

### 2.1.2. `Data/ApplicationDbContextFactory.cs`

Vai tro:

1. Ho tro EF Core tooling khi chay migration.
2. Dam bao `dotnet ef` co the tao `ApplicationDbContext` ngay ca khi app chua chay.

### 2.1.3. `Data/IdentitySeedData.cs`

Vai tro:

1. Tao cac role can thiet neu chua ton tai.
2. Tao tai khoan admin va employee mac dinh.
3. Dam bao he thong moi khoi tao van co nguoi vao admin area ngay lap tuc.

Gia tri quan trong:

1. `AdminEmail`
2. `AdminPassword`
3. `EmployeeEmail`
4. `EmployeePassword`

Tai sao phai seed:

1. Neu khong co admin ban dau, he thong da co admin area nhung khong co ai truy cap duoc.
2. Tai lieu huong dan va demo se de dang hon khi luon co account co san.

## 2.2. Domain Models

## 2.2.1. Identity

### `Models/Identity/ApplicationUser.cs`

`ApplicationUser` mo rong `IdentityUser`, tuc la ngoai cac truong mac dinh cua Identity nhu `Id`, `Email`, `PasswordHash`, `PhoneNumber`, he thong bo sung them du lieu phuc vu nghiep vu ecommerce.

Bien chinh:

1. `FullName`
2. `DefaultAddressLine1`
3. `DefaultAddressLine2`
4. `DefaultCity`
5. `DefaultProvince`
6. `DefaultPostalCode`
7. `CreatedAtUtc`

Tai sao can cac bien default address:

1. Checkout co the prefill thong tin giao hang.
2. User khong phai nhap lai dia chi moi lan dat hang.
3. Profile va checkout lien ket voi nhau thong qua du lieu nay.

### `Models/Identity/ApplicationRole.cs`

Co 2 khai niem:

1. Enum `ApplicationRole`
2. Static class `ApplicationRoles`

Gia tri role dang dung:

1. `Admin`
2. `Employee`
3. `User`

Tai sao co static class `ApplicationRoles`:

1. Tranh viet chuoi role thu cong o nhieu noi.
2. Giam loi typo trong `[Authorize(Roles = ...)]`, seed va user management.

## 2.2.2. Catalog

### `Models/Catalog/Category.cs`

Entity danh muc dung de nhom san pham.

Bien chinh:

1. `Id`
2. `Name`
3. `Description`
4. `DisplayOrder`
5. `IsActive`
6. `CreatedAtUtc`
7. `UpdatedAtUtc`
8. `Products`

Tai sao can `DisplayOrder` va `IsActive`:

1. `DisplayOrder` cho phep admin sap xep category theo y muon thay vi thu tu ID.
2. `IsActive` cho phep tam an category tren storefront ma khong can xoa du lieu.

### `Models/Catalog/Product.cs`

Day la entity trung tam cua module catalog.

Bien chinh:

1. `Id`
2. `Name`
3. `Sku`
4. `Description`
5. `Price`
6. `IsDiscountActive`
7. `DiscountPercentage`
8. `StockQuantity`
9. `Status`
10. `CategoryId`
11. `Category`
12. `CreatedAtUtc`
13. `UpdatedAtUtc`
14. `Images`
15. `Reviews`

Gia tri dan xuat quan trong:

1. `HasActiveDiscount`
2. `EffectivePrice`
3. `DiscountAmount`

Tai sao `Status` quan trong:

1. Storefront chi hien product `Active`.
2. Admin van co the sua product inactive.
3. Gio hang, wishlist, review, product detail deu dua vao status nay de quyet dinh co cho thao tac hay khong.

Tai sao them direct discount vao `Product`:

1. Muc giam gia tro thanh thuoc tinh cua tung san pham thay vi chi la promo code toan cuc.
2. Homepage, product list, product detail, cart, wishlist, checkout va order co the cung doc mot nguon gia hieu luc.
3. Admin co the quan ly giam gia ngay trong man hinh product ma khong can tao mot module flash sale rieng.

### `Models/Catalog/ProductImage.cs`

Vai tro:

1. Mot product co nhieu anh.
2. `DisplayOrder` quyet dinh thu tu hien thi.
3. `IsPrimary` xac dinh anh dai dien.

Tai sao tach thanh bang rieng:

1. Product co the co nhieu anh.
2. De mo rong gallery, thumbnail, anh chinh ma khong lam `Product` qua nang.

### `Models/Catalog/ProductReview.cs`

Entity review cua san pham.

Bien chinh:

1. `ProductId`
2. `UserId`
3. `Rating`
4. `Comment`
5. `Status`
6. `CreatedAtUtc`
7. `UpdatedAtUtc`

Tai sao review co `Status`:

1. Review khong duoc public ngay lap tuc.
2. Admin co the moderation.
3. Chi review approved moi ra storefront.

### `Models/Catalog/WishlistItem.cs`

Entity lien ket `User` va `Product`.

Bien chinh:

1. `UserId`
2. `ProductId`
3. `CreatedAtUtc`

Tai sao wishlist dung DB ma cart dung session:

1. Wishlist la du lieu ca nhan, can ton tai lau dai va theo user account.
2. Cart la du lieu tam thoi, can chay duoc ca voi user an danh.

### Enum catalog

1. `ProductStatus`
2. `ProductReviewStatus`

Y nghia:

1. `ProductStatus` dieu khien product co duoc ban cong khai hay khong.
2. `ProductReviewStatus` dieu khien review co duoc hien cong khai hay khong.

## 2.2.3. Orders

### `Models/Orders/Order.cs`

Day la aggregate quan trong nhat cua module dat hang. No luu snapshot cua mot giao dich mua hang tai thoi diem checkout.

Nhom bien chinh:

1. Dinh danh:
   - `Id`
   - `OrderNumber`
   - `UserId`

2. Trang thai xu ly:
   - `Status`
   - `PaymentMethod`
   - `PaymentStatus`

3. Thanh toan:
   - `PaymentProvider`
   - `PaymentTransactionReference`
   - `PaymentFailureReason`

4. Dia chi giao hang:
   - `ShippingFullName`
   - `ShippingEmail`
   - `ShippingPhone`
   - `ShippingAddressLine1`
   - `ShippingAddressLine2`
   - `ShippingCity`
   - `ShippingProvince`
   - `PostalCode`

5. Shipping / tracking:
   - `ShipmentCarrier`
   - `TrackingNumber`
   - `TrackingUrl`

6. Pricing snapshot:
   - `Subtotal`
   - `PromoCode`
   - `PromoDiscountPercentage`
   - `DiscountAmount`
   - `ShippingFee`
   - `TaxRate`
   - `TaxAmount`
   - `CurrencyCode`
   - `TotalAmount`

7. Co dieu khien nghiep vu huy / hoan tien:
   - `IsInventoryRestored`
   - `IsRefundReady`

8. Cac moc thoi gian:
   - `CreatedAtUtc`
   - `PaymentAuthorizedAtUtc`
   - `PaidAtUtc`
   - `EstimatedDeliveryDateUtc`
   - `ShippedAtUtc`
   - `DeliveredAtUtc`
   - `CancelledAtUtc`
   - `RefundReadyAtUtc`
   - `RefundedAtUtc`
   - `UpdatedAtUtc`

9. Quan he:
   - `Items`
   - `StatusHistory`

Tai sao Order phai luu snapshot day du:

1. Sau khi dat hang, gia product co the thay doi.
2. Ten product, promo, phi ship, thue tai thoi diem dat phai duoc dong bang.
3. Don hang can luu duoc lich su that, khong phu thuoc vao state hien tai cua product.

### `Models/Orders/OrderItem.cs`

`OrderItem` khong phai la line tham chieu song song toi product hien tai, ma la snapshot dong cua san pham trong thoi diem checkout.

Bien chinh:

1. `ProductId`
2. `ProductName`
3. `ProductSku`
4. `UnitPrice`
5. `Quantity`

Tai sao phai luu `ProductName` va `ProductSku` ngay trong `OrderItem`:

1. Neu sau nay admin doi ten product hoac SKU, don cu van hien thi dung du lieu tai thoi diem mua.

### `Models/Orders/OrderStatusHistory.cs`

Vai tro:

1. Luu audit trail thay doi trang thai.
2. Cho phep nguoi dung va admin xem hanh trinh don hang.

Bien:

1. `PreviousStatus`
2. `NewStatus`
3. `Note`
4. `ChangedByUserId`
5. `ChangedByName`
6. `ChangedAtUtc`

### Enum orders

1. `OrderStatus`: `Pending`, `Processing`, `Completed`, `Cancelled`
2. `PaymentMethod`: `CashOnDelivery`, `DemoGateway`
3. `PaymentStatus`: `Pending`, `Authorized`, `Paid`, `Failed`, `Refunded`

Tai sao tach 3 enum nay:

1. `OrderStatus` mo ta trang thai fulfillment.
2. `PaymentStatus` mo ta trang thai thanh toan.
3. `PaymentMethod` mo ta kenh thanh toan.
4. Ba khai niem nay co lien quan nhung khong dong nghia.

## 2.2.4. Settings

### `Models/Settings/SystemSetting.cs`

Day la cau hinh toan cuc cua storefront.

Nhom bien chinh:

1. Branding:
   - `StoreName`
   - `SupportEmail`
   - `LogoImagePath`

2. Hero:
   - `HeroBadgeText`
   - `HeroTitle`
   - `HeroSubtitle`
   - `HeroPrimaryButtonText`
   - `HeroPrimaryButtonUrl`
   - `HeroSecondaryButtonText`
   - `HeroSecondaryButtonUrl`

3. Promo:
   - `IsPromoBannerActive`
   - `PromoTitle`
   - `PromoSubtitle`
   - `PromoButtonText`
   - `PromoButtonUrl`
   - `PromoCode`
   - `PromoDiscountPercentage`

4. Pricing:
   - `StandardShippingFee`
   - `FreeShippingThreshold`
   - `TaxRatePercentage`
   - `CurrencyCode`

5. Khac:
   - `PaymentInstructions`
   - `SmtpHost`
   - `SmtpPort`
   - `SmtpUsername`
   - `SmtpFromEmail`
   - `SeoMetaTitle`
   - `SeoMetaDescription`
   - `DefaultCulture`
   - `ShowHomepageSlider`
   - `ShowHomepageCategories`
   - `ShowHomepageNewProducts`
   - `ShowHomepageFeaturedProducts`
   - `ShowHomepageDiscountProducts`

Tai sao `SystemSetting` la row duy nhat:

1. He thong hien dang thiet ke cho mot storefront.
2. Toan bo layout, homepage section toggle, promo, pricing va meta SEO doc tu mot nguon chung.
3. Don gian hoa admin settings va logic render.

Luu y ve hero cu:

1. Cac truong `Hero*` van ton tai de giu tuong thich du lieu cu.
2. `SystemSettingService.EnsureDefaultAsync()` co the seed mot `SliderItem` banner mac dinh tu du lieu hero cu.
3. Storefront moi uu tien doc `SliderItems` thay vi render hero co dinh.

### `Models/Settings/SliderItem.cs`

Day la entity moi dung cho slider trang chu.

Bien chinh:

1. `Id`
2. `Type`
3. `Title`
4. `Description`
5. `Content`
6. `PrimaryButtonText`
7. `PrimaryButtonUrl`
8. `SecondaryButtonText`
9. `SecondaryButtonUrl`
10. `BackgroundImagePath`
11. `ClickUrl`
12. `DisplayOrder`
13. `IsActive`
14. `CreatedAtUtc`
15. `UpdatedAtUtc`

Enum lien quan:

1. `SliderItemType.Banner`
2. `SliderItemType.RegularSlide`

Tai sao tach `SliderItem` khoi `SystemSetting`:

1. Banner/slide la du lieu lap lai theo danh sach, khong phu hop voi mot row settings duy nhat.
2. Admin can CRUD, sap xep va bat/tat tung slide.
3. Banner va regular slide co bo field va rang buoc khac nhau.

## 2.2.5. API DTO

DTO API chinh:

1. `CategorySummaryDto`
2. `ProductSummaryDto`
3. `ProductDetailDto`
4. `OrderTrackingDto`

Tai sao can DTO rieng thay vi tra thang entity:

1. Tranh lo du lieu noi bo.
2. Co the dinh dang output dung nhu public API can.
3. Tranh vong tham chieu navigation property khi serialize.

## 2.3. ViewModel Layer

ViewModel la lop trung gian giua controller va view, khong dong nhat voi entity DB.

Tai sao du an can ViewModel:

1. Form nhap lieu va man hinh hien thi co cau truc rieng.
2. Khong nen bind truc tiep entity tu form.
3. Giup validation, presentation va nghiep vu duoc tach ro.

Nhom ViewModel chinh:

1. Auth
2. Home / Product / Review
3. Cart
4. Order
5. Profile
6. Wishlist
7. Admin
8. Settings public

## 2.4. Services

Services la noi giu phan lon logic nghiep vu cua he thong. Controller thuong chi xu ly 4 viec:

1. nhan input
2. validate co ban
3. goi service
4. quyet dinh view/redirect

Service chinh:

1. `AccountService`
2. `AdminDashboardService`
3. `CartService`
4. `CategoryService`
5. `OrderService`
6. `DemoPaymentGatewayService`
7. `ProductService`
8. `RealtimeNotificationService`
9. `ReviewService`
10. `SystemSettingService`
11. `UserAdminService`
12. `WishlistService`

Tai sao day la quyet dinh kien truc tot:

1. Service co the duoc test rieng voi controller.
2. Giam duplicate logic giua public area, admin area va API.
3. Khi can thay doi nghiep vu, chi sua o mot noi trung tam.

## 2.5. Controllers

Controller duoc chia thanh 3 nhom:

1. Public MVC
2. API
3. Admin MVC

Tai sao tach Admin Area:

1. Router ro rang hon.
2. View admin tach biet storefront.
3. De ap chinh sach role rieng.

## 2.6. Resources

File:

1. `Resources/SharedResource.cs`
2. `Resources/SharedResource.en.resx`
3. `Resources/SharedResource.vi.resx`

Vai tro:

1. Cung cap chuoi da ngon ngu cho controller va view.
2. Dam bao he thong chuyen ngon ngu ma khong phai hard-code text.

## 2.7. Realtime Layer

Trung tam la `Hubs/NotificationHub.cs`.

Vai tro:

1. Gan ket noi SignalR vao group theo `userId`.
2. Gan admin vao group `role:admin`.
3. Cho phep gui thong bao tao don va cap nhat don den dung doi tuong.

## 2.8. Views

Views duoc chia:

1. Shared
2. Auth
3. Public storefront
4. Admin

File quan trong nhat o tang giao dien la `Views/Shared/_Layout.cshtml` vi day la shell chung cua storefront.

## 3. Bootstrap He Thong Trong Program.cs

`Program.cs` la noi lap rap toan bo he thong. Neu hinh dung ung dung nhu mot nha may, thi `Program.cs` la noi khoi dong day chuyen va ket noi tat ca module.

## 3.1. Localization

Bien va cau hinh:

1. `supportedCultures = ["en", "vi"]`
2. `AddLocalization()`
3. `AddViewLocalization()`
4. `UseRequestLocalization(...)`

Tai sao can localization o muc bootstrap:

1. Middleware phai biet request hien tai dang dung culture nao truoc khi controller va view chay.
2. Layout, validation, tempdata message, label va button deu phu thuoc culture.

## 3.2. Database va EF Core

`AddDbContext<ApplicationDbContext>(UseSqlServer(...))`

Tai sao dang ky o day:

1. Moi request can nhan `DbContext` qua DI.
2. Service co the goi DB ma khong tu tao connection thu cong.

## 3.3. Identity

`AddIdentity<ApplicationUser, IdentityRole>(...)`

Chinh sach password dang duoc bat:

1. `RequiredLength = 6`
2. bat buoc digit
3. bat buoc lowercase
4. bat buoc uppercase
5. bat buoc non-alphanumeric
6. `RequireUniqueEmail = true`

Tai sao quan trong:

1. Dang ky, dang nhap, doi mat khau, reset password deu dua tren cau hinh nay.
2. Admin user management va seed role cung dua tren he thong Identity nay.

## 3.4. Cookie Auth

Gia tri quan trong:

1. `LoginPath = /Auth/Login`
2. `AccessDeniedPath = /Auth/AccessDenied`
3. `LogoutPath = /Auth/Logout`
4. `Cookie.Name = ASPNET_Ecommerce.Auth`

Tai sao can noi ro:

1. Khi user chua login ma vao route `[Authorize]`, middleware se dua ve login.
2. Khi khong du role, middleware dua sang access denied.

## 3.5. Session

Gia tri:

1. `Cookie.Name = ASPNET_Ecommerce.Session`
2. `IdleTimeout = 4 gio`

Tai sao session quan trong trong du an nay:

1. Gio hang duoc luu o session.
2. User an danh van co the mua sam.

## 3.6. Dang ky DI cho Services

Tat ca service nghiep vu duoc dang ky o day. Neu service khong duoc dang ky thi controller khong the inject va app se khong chay.

## 3.7. SignalR

1. `AddSignalR()`
2. `MapHub<NotificationHub>("/hubs/notifications")`

Tai sao map route hub o day:

1. Client JS can mot endpoint co dinh de mo ket noi realtime.

## 3.8. Migrate va Seed

Khi app khoi dong:

1. `Database.MigrateAsync()`
2. `SystemSettingService.EnsureDefaultAsync()`
3. `IdentitySeedData.SeedAsync(app.Services)`

Y nghia:

1. Dong bo schema.
2. Tao settings mac dinh.
3. Tao role va tai khoan mac dinh.

## 3.9. Routing

Co 2 nhanh chinh:

1. route cho area admin
2. route MVC mac dinh `{controller=Home}/{action=Index}/{id?}`

## 4. Luong Chuc Nang Chi Tiet

## 4.1. Xac Thuc Va Phan Quyen

## 4.1.1. Dang ky tai khoan

Muc dich:

1. Tao `ApplicationUser` moi.
2. Gan role `User`.
3. Dang nhap ngay sau khi dang ky thanh cong.

Diem vao:

1. `GET /Auth/Register`
2. `POST /Auth/Register`

Du lieu vao:

1. `RegisterViewModel.FullName`
2. `RegisterViewModel.Email`
3. `RegisterViewModel.Password`
4. cac truong xac nhan mat khau trong form

Thanh phan tham gia:

1. `AuthController`
2. `_userManager`
3. `_signInManager`
4. `ApplicationRoles.User`

Luong xu ly:

1. GET chi tra ve view model rong de render form.
2. POST kiem tra `ModelState`.
3. Tim user theo email de tranh dang ky trung.
4. Tao doi tuong `ApplicationUser` moi.
5. Goi `_userManager.CreateAsync(user, model.Password)`.
6. Neu Identity tra loi, controller dua loi vao `ModelState`.
7. Neu tao thanh cong, gan role `User`.
8. Dang nhap ngay voi `_signInManager.SignInAsync(user, false)`.
9. Redirect ve `Home/Index`.

Tai sao he thong dang nhap ngay sau dang ky:

1. Giam ma sat cho user.
2. User co the mua hang, wishlist, review ngay lap tuc.

## 4.1.2. Dang nhap

Muc dich:

1. Xac thuc user bang email + password.
2. Tao auth cookie.

Diem vao:

1. `GET /Auth/Login`
2. `POST /Auth/Login`

Du lieu vao:

1. `Email`
2. `Password`
3. `RememberMe`
4. `ReturnUrl`

Luong xu ly:

1. GET render form va giu `ReturnUrl`.
2. POST tim user theo email.
3. Neu khong co user, tra loi chung ve invalid login.
4. Goi `_signInManager.PasswordSignInAsync(...)`.
5. Neu that bai, tra lai view cung thong bao loi.
6. Neu thanh cong, redirect ve `ReturnUrl` neu la local URL.
7. Neu khong co `ReturnUrl`, quay ve home.

Tai sao phai kiem tra local URL:

1. Tranh open redirect.
2. Bao dam redirect sau login chi quay ve noi bo he thong.

## 4.1.3. Dang xuat

Luong:

1. `POST /Auth/Logout`
2. `_signInManager.SignOutAsync()`
3. Redirect ve `Home/Index`

Tai sao logout dung POST:

1. Tranh bi kich hoat boi link GET ngoai y muon.
2. Ket hop anti-forgery an toan hon.

## 4.1.4. Quen mat khau va dat lai mat khau

Thanh phan:

1. `AuthController`
2. `AccountService.GeneratePasswordResetTokenAsync(...)`
3. `AccountService.ResetPasswordAsync(...)`

Luong quen mat khau:

1. User nhap email.
2. Service tim user theo email.
3. Neu khong co user, tra `null` va UI van khong tiet lo qua nhieu thong tin.
4. Neu co, Identity tao reset token.
5. Token duoc encode bang `Base64UrlEncode`.
6. O moi truong development, controller tao `DevelopmentResetUrl` de de test.

Luong dat lai mat khau:

1. User mo link reset co email + token.
2. Controller tao `ResetPasswordViewModel`.
3. Service decode token bang `Base64UrlDecode`.
4. Goi `_userManager.ResetPasswordAsync(...)`.
5. Neu token hong hoac het han, service nem `InvalidOperationException`.

Tai sao token duoc encode:

1. Token Identity thuong chua ky tu dac biet.
2. Encode giup dua vao URL an toan hon.

## 4.1.5. Phan quyen

Co che:

1. `[Authorize]` cho chuc nang can dang nhap.
2. `[Authorize(Roles = ...)]` cho admin/employee.
3. Claims role duoc doc tu auth cookie sau login.

Tai sao phan quyen la tang che chan som:

1. Request bi chan ngay tai controller level truoc khi logic nghiep vu chay.
2. Giam nguy co goi nham service voi user khong hop le.

## 4.2. Seed Role, User Mac Dinh va Settings Mac Dinh

## 4.2.1. Seed Identity

Luong:

1. `Program.cs` goi `IdentitySeedData.SeedAsync(...)`.
2. `RoleManager` tao role neu chua ton tai.
3. `UserManager` tim admin theo email.
4. Neu chua co thi tao admin moi.
5. Gan role `Admin` neu thieu.
6. Lap lai voi employee mac dinh.

Tai sao can vua tao user vua bao dam role mapping:

1. User ton tai nhung khong co role van khong vao dung area duoc.

## 4.2.2. Seed settings

Luong:

1. `SystemSettingService.GetOrCreateAsync()` doc row settings.
2. Neu chua co, tao mot row mac dinh day du.
3. `EnsureDefaultAsync()` dam bao bootstrap xong la storefront render duoc.

Tai sao can row settings mac dinh:

1. Layout can ten shop va metadata ngay o request dau tien.
2. Checkout can currency, shipping, tax.
3. Home can hero/promo.

## 4.3. Quan Ly Danh Muc

Muc dich:

1. Cho admin quan ly nhom san pham.
2. Cap du lieu cho storefront bo loc category.

Thanh phan:

1. `Areas/Admin/Controllers/CategoriesController`
2. `CategoryService`
3. `CategoryFormViewModel`

Luong tao:

1. GET tao form rong.
2. POST nhan `CategoryFormViewModel`.
3. Controller kiem tra `ModelState`.
4. Service `EnsureUniqueNameAsync(...)` dam bao khong trung ten.
5. Tao entity `Category`, trim chuoi, set `CreatedAtUtc`.
6. Save DB.
7. Controller gan `TempData["StatusMessage"]` roi redirect.

Luong sua:

1. GET doc category theo id va map sang viewmodel.
2. POST kiem tra id route va model.
3. Service load entity, kiem tra trung ten, cap nhat du lieu.
4. Set `UpdatedAtUtc`.
5. Save DB.

Luong xoa:

1. Service load category kem `Products`.
2. Neu category con san pham, nem `InvalidOperationException`.
3. Neu rong, remove category.

Tai sao khong cho xoa category con san pham:

1. Tranh tao product mo coi category.
2. Giu toan ven du lieu catalog.

## 4.4. Quan Ly San Pham

Muc dich:

1. Quan ly catalog ban hang.
2. Xu ly upload gallery product.

Thanh phan:

1. `ProductsController`
2. `ProductService`
3. `ProductFormViewModel`
4. `ProductImageViewModel`

Bien noi bo quan trong trong service:

1. `AllowedImageExtensions`
2. `MaxImageSizeBytes = 5MB`
3. `UploadFolder = uploads/products`

Luong tao product:

1. GET goi `BuildCreateModelAsync()`.
2. Service tao model rong va `PopulateLookupsAsync()` de nap category dropdown.
3. POST nhan form.
4. Neu invalid, controller goi lai `PopulateLookupsAsync()` de view khong mat dropdown data.
5. `CreateAsync(model)` kiem tra category ton tai.
6. `SaveUploadedFilesAsync(...)` validate va luu file vao `wwwroot/uploads/products`.
7. Tao `Product`.
8. Tao danh sach `ProductImage`, anh dau tien duoc danh dau primary.
9. Save DB.

Luong sua product:

1. GET load product kem images.
2. Map sang `ProductFormViewModel`.
3. POST cap nhat field text, price, stock, status, category.
4. Xoa anh nam trong `RemovedImageIds`.
5. Upload anh moi.
6. Dat `IsPrimary` theo `PrimaryImageId`.
7. Neu khong con anh primary, chon anh dau tien theo `DisplayOrder`.
8. Save DB.
9. Sau khi save moi xoa file vat ly da bi bo, tranh mat file neu DB save that bai.

Luong xoa product:

1. Load product kem anh.
2. Remove product khoi DB.
3. Save DB.
4. Xoa file anh vat ly.

Tai sao file duoc xoa sau khi DB save thanh cong:

1. Neu xoa file truoc ma save DB loi, DB va file system se lech nhau.

Tai sao store public chi doc product active:

1. Admin can product draft/inactive de quan ly noi bo.
2. Khach hang chi nen thay du lieu da san sang ban.

## 4.5. Trang Chu Storefront

Thanh phan:

1. `HomeController.Index()`
2. `_categoryService.GetActiveCategoriesAsync()`
3. `_productService.GetFeaturedAsync(8)`
4. `_productService.GetNewArrivalsAsync(8)`
5. `_productService.GetDiscountedAsync(8)`
6. `_systemSettingService.GetPublicAsync()`

Luong:

1. `HomeController` doc `StorefrontSettingsViewModel` tu `SystemSettingService`.
2. Neu `ShowHomepageSlider = true`, settings public kem theo danh sach `SliderItems` dang active, sap xep theo `DisplayOrder`.
3. Neu `ShowHomepageCategories = true`, load category active.
4. Neu `ShowHomepageNewProducts = true`, load `GetNewArrivalsAsync(8)`.
5. Neu `ShowHomepageFeaturedProducts = true`, load `GetFeaturedAsync(8)`.
6. Neu `ShowHomepageDiscountProducts = true`, load `GetDiscountedAsync(8)`.
7. Controller gop tat ca vao `HomeViewModel` va tra `Views/Home/Index.cshtml`.

Luong render slider:

1. `Index.cshtml` duyet `Model.Settings.SliderItems`.
2. `SliderItemType.Banner` hien title, description, content, button chinh, button phu va background.
3. Neu banner khong co `BackgroundImagePath`, view hien background mac dinh bang gradient.
4. `SliderItemType.RegularSlide` render anh nen, va neu co `ClickUrl` thi cho phep bam vao.
5. JavaScript tren view tu xoay slide, nhung van ton trong danh sach active va thu tu do admin sap xep.

Tai sao Home khong tu query DB truc tiep:

1. Logic chon slider, section toggle, featured, new arrivals va discounted products duoc tap trung vao service.
2. Controller ngan gon va de bao tri hon.

## 4.6. Danh Sach San Pham Cong Khai

Thanh phan:

1. `ProductController.Index(...)`
2. `ProductService.GetActiveAsync(...)`

Input filter:

1. `search`
2. `categoryId`
3. `sortBy`
4. `minPrice`
5. `maxPrice`
6. `inStockOnly`
7. `page`

Luong trong controller:

1. Chuan hoa `page >= 1`.
2. Lay danh sach category active cho bo loc.
3. Goi `GetActiveAsync(...)` lay danh sach product da phan trang.
4. Goi lai `GetActiveAsync(page: 1, pageSize: int.MaxValue)` de xac dinh khoang gia min/max phuc vu UI filter.
5. Tao `ProductListViewModel`.

Luong trong service:

1. Bat dau tu `Products.Where(Status == Active)`.
2. Neu co `search`, loc theo `Name`, `Sku`, `Description`.
3. Neu co `categoryId`, loc theo category.
4. Khoang gia `minPrice`, `maxPrice` duoc ap tren `EffectivePrice`, khong phai `Price` goc.
5. Neu `inStockOnly`, loc `StockQuantity > 0`.
6. Ap dung sort.
7. Dem `totalCount`.

Tai sao product list loc theo `EffectivePrice`:

1. User dang mua theo gia thuc te, khong phai gia niem yet truoc giam.
2. Neu van loc theo `Price` goc, bo loc gia va thu tu sap xep se sai voi so tien hien tren UI.
8. `Skip/Take` theo trang.
9. `Include(Category, Images)`.

Tai sao `GetActiveAsync` vua tra items vua tra totalCount:

1. UI phan trang can tong so item de tinh tong so trang.
2. Neu khong tra `totalCount`, controller se phai query them mot lan nua.

## 4.7. Chi Tiet San Pham

Thanh phan:

1. `ProductController.Details(int id)`
2. `ProductService.GetByIdAsync(id)`
3. `ProductService.GetRelatedAsync(...)`
4. `ReviewService`
5. `WishlistService`

Du lieu duoc tong hop trong action nay lon hon mot action detail thong thuong vi no phai render toan bo trang chi tiet.

Luong:

1. Lay product active theo id.
2. Neu khong co, `NotFound()`.
3. Lay product lien quan cung category.
4. Lay danh sach review da public.
5. Lay thong ke review: trung binh sao, so luong review.
6. Neu user login:
   - kiem tra product co trong wishlist khong
   - kiem tra user da review chua
   - kiem tra user co du dieu kien review khong
7. Tao `ProductDetailViewModel`.
8. View render gallery, thong tin, wishlist button, add-to-cart form, related products va block review.

Tai sao detail page phai tong hop nhieu service:

1. Day la diem giao cua catalog, cart, wishlist, review.
2. Toan bo trang quyet dinh kha nang mua hang va tuong tac cua user.

## 4.8. Review San Pham

Muc dich:

1. Chi user co mua hang that moi duoc review.
2. Review duoc dang cong khai ngay sau khi gui neu hop le.
3. Admin van co the an review hoac them phan hoi tu admin.

Thanh phan:

1. `ProductController.SubmitReview(...)`
2. `ReviewService.CreateAsync(...)`
3. `ReviewService.GetReviewEligibilityAsync(...)`

Luong:

1. Controller nhan `ReviewFormViewModel`.
2. Neu `ModelState` invalid, gan `TempData["ReviewError"]` va redirect ve detail.
3. Lay `userId` tu claim.
4. Neu khong co userId, `Challenge()`.
5. Service kiem tra dieu kien review:
   - product co ton tai va active khong
   - user da co review cho product nay chua
   - user co don `Completed` chua product nay khong
6. Neu hop le, tao `ProductReview` voi `Status = Approved`.
7. Save DB.
8. Controller set `ReviewSuccess`.

Tai sao review moi duoc dang thang:

1. Giam ma sat cho khach da mua hang xac minh.
2. Dieu kien verified purchase da giam bot review rac tu dau.
3. Admin van co quyen an review va tra loi khi can xu ly noi dung.

## 4.9. Quan Ly Review

Thanh phan:

1. `Admin/ReviewsController`
2. `ReviewService.GetAdminListAsync(...)`
3. `ReviewService.GetAdminDetailsAsync(...)`
4. `ReviewService.UpdateStatusAsync(...)`
5. `ReviewService.DeleteAsync(...)`

Luong:

1. Admin vao danh sach review.
2. He thong ho tro loc theo keyword va status.
3. Admin mo chi tiet review.
4. Admin co the doi status thanh `Approved` hoac `Hidden` va luu phan hoi admin.
5. He thong luu `UpdatedAtUtc` va `AdminRepliedAtUtc` neu co phan hoi.

Tai sao storefront chi doc review approved:

1. Public view khong nen phai tu biet moderation logic.
2. Service public review chi tra ra phan dang duoc phep hien cong khai.

## 4.10. Wishlist

Muc dich:

1. Luu san pham yeu thich theo tai khoan.
2. Cho phep user quay lai xem sau.

Thanh phan:

1. `WishlistController`
2. `WishlistService`
3. `WishlistMutationViewModel`

Du lieu vao form:

1. `ProductId`
2. `ReturnUrl`

Luong them wishlist:

1. Controller dam bao user dang nhap.
2. Kiem tra `ModelState`.
3. Service kiem tra item da ton tai chua.
4. Neu da ton tai, bo qua de tranh duplicate.
5. Kiem tra product ton tai va active.
6. Tao `WishlistItem`.
7. Save DB.
8. Controller set `TempData` va redirect ve `ReturnUrl` local neu co.

Luong bo wishlist:

1. Tim item theo `UserId + ProductId`.
2. Neu co thi xoa.
3. Save DB.

Tai sao wishlist dung unique index `(UserId, ProductId)`:

1. Tranh trung lap cap du lieu.
2. Don gian hoa logic dem wishlist va check da yeu thich chua.

Tai sao navbar co the hien badge wishlist:

1. `_Layout.cshtml` chi goi `WishlistService.GetItemCountAsync(userId)` neu user dang nhap.
2. Gia tri nay doc tu DB, khong doc session.
3. Vi layout duoc render lai moi request, badge wishlist tu dong cap nhat sau khi add/remove roi redirect.

## 4.11. Gio Hang

Muc dich:

1. Luu lua chon mua tam thoi cua user.
2. Hoat dong duoc voi user an danh.
3. Dong bo voi ton kho that tai thoi diem render.

Thanh phan:

1. `CartController`
2. `CartService`
3. `Views/Cart/Index.cshtml`
4. `Views/Shared/_Layout.cshtml`

Nguon du lieu chinh:

1. Session key: `cart.items`
2. Moi phan tu session gom:
   - `ProductId`
   - `Quantity`

Tai sao gio hang dung session thay vi DB:

1. User chua dang nhap van co the bo hang vao gio.
2. Session nhanh, don gian, du cho nhu cau tam thoi.
3. Chi den checkout moi can xac dinh user va tao du lieu ben vung trong DB.

### Luong them vao gio

1. User submit form `AddToCartViewModel` tu trang chi tiet san pham.
2. `CartController.Add(model)` kiem tra `ModelState`.
3. Neu invalid, gan `TempData["CartError"]` va redirect.
4. Neu hop le, controller goi `CartService.AddItemAsync(model.ProductId, model.Quantity)`.
5. Service kiem tra `quantity > 0`.
6. Goi `GetAvailableProductAsync(productId)` de dam bao product van active va con ton kho.
7. Service doc session bang `GetSessionItems()`.
8. Neu product da co san trong gio, lay `currentQuantity`; neu chua co thi xem nhu `0`.
9. Tinh `nextQuantity = currentQuantity + quantity`.
10. Neu `nextQuantity > product.StockQuantity`, nem loi nghiep vu.
11. Neu chua co item, them `CartSessionItem` moi.
12. Neu da co, cap nhat `existingItem.Quantity = nextQuantity`.
13. Goi `SaveSessionItems(sessionItems)`.
14. Controller set `TempData["CartSuccess"]`.
15. Redirect ve `ReturnUrl` local neu co, neu khong quay lai `Product/Details/{id}`.

### Vi sao badge gio hang tren navbar thay doi duoc

1. Source of truth cua gio hang khong nam o HTML, ma nam trong session `cart.items`.
2. Sau khi add item, controller redirect tao ra mot HTTP request moi.
3. O request moi do, `Views/Shared/_Layout.cshtml` duoc render lai.
4. Ngay dau layout co dong `var cartItemCount = await CartService.GetItemCountAsync();`.
5. `GetItemCountAsync()` doc session va tinh `Sum(item => item.Quantity)`.
6. Gia tri tong nay duoc render vao badge do gan icon gio hang.
7. Vi vay UI khong can JavaScript polling van cap nhat dung.

### Tai sao badge hien tong quantity thay vi tong so dong san pham

1. Service tinh tong bang `Sum(item => item.Quantity)`.
2. Neu co 2 ao va 3 quan, badge hien `5`.
3. Day la tong don vi hang, khong phai tong so loai hang.
4. Cach nay phan anh dung hon quy mo gio hang ma user thuc su sap mua.

### Luong doc gio hang

1. `CartController.Index()` goi `CartService.GetCartAsync()`.
2. Service doc session `cart.items`.
3. Neu rong, tra `CartIndexViewModel` rong.
4. Neu co du lieu, service lay `productIds` tu session.
5. Query DB lay cac product active co id tuong ung, include images.
6. Vong lap qua product de tao danh sach `CartItemViewModel`.
7. Neu product khong con hop le hoac het hang, bo qua item do.
8. Neu quantity vuot ton kho, cat xuong bang ton kho thuc te.
9. Neu session bi lech so voi ket qua chuan hoa, service ghi lai session moi.
10. Tinh `ItemCount` va `Subtotal`.

### Tai sao can buoc chuan hoa session trong `GetCartAsync()`

1. Session la ban tam giu o web app, co the cu hon du lieu thuc te trong DB.
2. Product co the bi inactive trong luc user chua mo lai gio hang.
3. Ton kho co the giam do user khac mua.
4. Neu khong chuan hoa, user se thay gio hang sai so luong hoac den checkout moi vo.
5. Chuan hoa som giup navbar, cart page va checkout dong bo voi ton kho that.

### Luong cap nhat so luong

1. `CartController.Update(model)` nhan `UpdateCartItemViewModel`.
2. Kiem tra `ModelState`.
3. Service kiem tra `quantity > 0`.
4. Load product active.
5. Neu vuot ton kho, nem loi.
6. Tim item trong session.
7. Cap nhat quantity.
8. Save session.

### Luong xoa khoi gio

1. `CartController.Remove(productId)` goi `RemoveItemAsync(productId)`.
2. Service xoa item trong session.
3. Neu gio rong, `SaveSessionItems([])` se xoa key session.

### Tai sao layout chi goi `GetItemCountAsync()` thay vi `GetCartAsync()`

1. Navbar chi can mot con so nho.
2. `GetCartAsync()` nang hon vi phai query DB, include image, tinh subtotal va co the chuan hoa.
3. `GetItemCountAsync()` nhanh hon vi chi doc JSON session va cong quantity.
4. Day la toi uu hop ly cho moi request.

## 4.12. Checkout Va Tao Don Hang

Muc dich:

1. Chuyen du lieu tam thoi trong gio hang thanh don hang ben vung trong DB.
2. Dong bang snapshot gia, shipping, tax, promo va item.

Thanh phan:

1. `OrderController`
2. `OrderService`
3. `CartService`
4. `DemoPaymentGatewayService`
5. `RealtimeNotificationService`
6. `OrderEmailService`

### GET checkout

1. User vao `GET /Order/Checkout`.
2. Controller lay `userId` tu claim.
3. `OrderService.BuildCheckoutAsync(userId)` load `ApplicationUser`.
4. Service prefill thong tin profile vao `CheckoutViewModel`.
5. `PaymentMethod` mac dinh la `CashOnDelivery`.
6. Service goi `PopulateCheckoutAsync(model)`.
7. `PopulateCheckoutAsync` doc gio hien tai, doc settings, tinh pricing snapshot.
8. Model checkout luc nay chua du du lieu de render trang.
9. Neu `HasItems = false`, controller redirect ve cart.

### Tai sao checkout duoc prefill tu profile

1. Giam thao tac nhap lai.
2. Tang toc do mua hang.
3. Gan ket profile va order flow.

### POST checkout

1. Controller nhan `CheckoutViewModel` tu form.
2. Ngay lap tuc goi `PopulateCheckoutAsync(model)` de tinh lai tong tien tren server.
3. Day la buoc cuc ky quan trong vi khong tin so tien gui len tu client.
4. Neu gio hang rong, redirect ve cart.
5. Neu `ModelState` invalid, tra lai view cung model da duoc repopulate.
6. `OrderService.CreateOrderAsync(userId, model)` thuc hien xu ly chinh.

### Ben trong `CreateOrderAsync(...)`

1. Load `ApplicationUser`.
2. Lay gio hien tai bang `_cartService.GetCartAsync()`.
3. Neu cart rong, nem loi.
4. Lay `productIds` tu cart.
5. Query DB lay product theo `productIds`.
6. Duyet tung `cartItem`:
   - product phai ton tai
   - product phai `Active`
   - `StockQuantity >= Quantity`
7. Doc `SystemSetting`.
8. Goi `BuildPricingSnapshot(cart.Subtotal, model.PromoCode, settings)`.
9. Tao `orderNumber` bang `GenerateOrderNumberAsync()`.
10. Goi `_paymentGatewayService.ProcessAsync(new PaymentGatewayRequest { ... })`.
11. Neu payment fail, nem `InvalidOperationException` va khong tao don.
12. Tao entity `Order` voi day du shipping snapshot, pricing snapshot, payment snapshot.
13. Duyet tung cart item de tao `OrderItem` voi `UnitPrice` lay tu gia hieu luc trong cart, khong dung `Product.Price` song song luc do.
14. Tru ton kho cua tung `Product`.
15. Ghi lich su trang thai bang `AddStatusHistory(...)`.
16. Neu user chon luu dia chi mac dinh, goi `SaveDefaultAddress(user, model, now)`.
17. Mo transaction DB.
18. Add order vao DB.
19. Save changes.
20. Commit transaction.
21. `_cartService.ClearAsync()` xoa gio session.
22. `_realtimeNotificationService.NotifyOrderCreatedAsync(...)` gui thong bao.
23. `_orderEmailService.TrySendOrderConfirmationAsync(order)` co gang gui mail xac nhan neu SMTP settings du thong tin.
24. Tra ve `order.Id`.

### Gui email sau checkout

1. `OrderEmailService` doc `SystemSetting` de lay `SmtpHost`, `SmtpPort`, `SmtpUsername`, `SmtpPassword`, `SmtpFromEmail`.
2. Neu thieu `SmtpHost` hoac `SmtpFromEmail`, service bo qua gui mail va khong lam fail checkout.
3. Neu co `SmtpUsername` nhung khong co `SmtpPassword`, service cung bo qua de tranh thu gui voi cau hinh nua vo.
4. Service tao `MailMessage` chua `OrderNumber`, tong tien, item da mua va link den trang `Order/Lookup`.
5. `SmtpClient.SendMailAsync(...)` duoc bao quanh bang logging, nen loi gui mail chi duoc ghi log, khong rollback don hang.

### Tai sao payment duoc xu ly truoc khi add order

1. He thong muon tranh tao order that bai thanh toan ngay tu dau.
2. Ket qua payment duoc dong goi vao snapshot order.

### Tai sao order phai dung transaction

1. Tao order, item, tru ton kho la mot cum thay doi nghiep vu.
2. Neu mot buoc fail ma buoc khac da luu, du lieu se lech.

### Tai sao cart bi clear sau khi commit thanh cong

1. Neu xoa gio truoc khi save don ma save fail, user se mat gio vo ly.
2. Clear sau commit dam bao user chi mat gio khi don da tao that.

### Direct discount trong cart va checkout di qua nhung dau moi nao

1. `ProductPricing` la noi tinh `EffectivePrice`, `DiscountAmount`, `HasActiveDiscount`.
2. `CartService` doc product active, tinh `UnitPrice` theo `ProductPricing` va luu them `OriginalUnitPrice`.
3. `OrderService.BuildCheckoutAsync(...)` va `PopulateCheckoutAsync(...)` lay subtotal tu cart da tinh discount.
4. `OrderService.CreateOrderAsync(...)` snapshot lai `cartItem.UnitPrice` vao `OrderItem.UnitPrice`.
5. Nho vay product list, product detail, cart, checkout, wishlist va order detail khong bi lech gia.

## 4.13. Tinh Gia, Promo, Thue, Phi Ship

Ham trung tam:

1. `OrderService.BuildPricingSnapshot(decimal subtotal, string? promoCodeInput, SystemSetting settings)`

Input:

1. `subtotal`
2. `promoCodeInput`
3. pricing settings tu `SystemSetting`

Luong:

1. Chuan hoa promo code nhap vao.
2. Kiem tra promo co bat va khop setting hay khong.
3. Tinh `discountAmount`.
4. Tinh `discountedSubtotal`.
5. Neu vuot `FreeShippingThreshold`, shipping fee = 0.
6. Neu khong, dung `StandardShippingFee`.
7. Tinh `taxAmount` tren `(discountedSubtotal + shippingFee)`.
8. Tinh `totalAmount`.
9. Tra ve `PricingSnapshot`.

Tai sao pricing phai tap trung vao mot ham:

1. Tranh tinh mot kieu o checkout, mot kieu o tao order.
2. Dam bao UI va DB snapshot cung dung mot cong thuc.

## 4.14. Confirmation, Lich Su Don, Chi Tiet Don

### Confirmation

1. `OrderController.Confirmation(id)` goi `GetConfirmationAsync(orderId, userId)`.
2. Service load order + items theo `orderId` va `userId`.
3. Map sang `OrderConfirmationViewModel`.
4. Neu la guest order vua tao xong, controller da grant session access qua `IGuestOrderAccessService` de guest xem lai `Confirmation`, `Details`, `Tracking` trong session hien tai.

Tai sao confirmation doc theo `orderId + userId`:

1. Tranh user xem don cua nguoi khac chi bang cach doi ID tren URL.

### Lich su don

1. `OrderController.Index()` goi `GetHistoryAsync(userId)`.
2. Query orders cua user.
3. Sap xep moi nhat truoc.
4. Projection ra `OrderHistoryItemViewModel`.

### Chi tiet don

1. `OrderController.Details(id)` goi `GetDetailsAsync(orderId, userId)`.
2. Service include `Items` va `StatusHistory`.
3. Map ra `OrderDetailsViewModel`.

### Tra cuu bang ma don

1. `GET /Order/Lookup` render form `OrderLookupViewModel`.
2. Form nhan `OrderNumber` va `Email`.
3. Neu user chua dang nhap, `Email` la bat buoc de verify guest order.
4. `OrderService.LookupAsync(orderNumber, userId, email)` query theo `OrderNumber`.
5. Neu order thuoc `userId` hien tai, user dang nhap duoc vao ma khong can email.
6. Neu order la guest (`UserId = null`), service chi cho qua khi `ShippingEmail` khop email nhap vao.
7. Sau khi lookup thanh cong mot guest order, controller goi `_guestOrderAccessService.GrantAccess(orderId)` de cho phep vao `Details` va `Tracking` trong session moi do.

Tai sao chi tiet don dung snapshot thay vi product hien tai:

1. Bao ton lich su giao dich.
2. Khong bi anh huong boi viec product bi doi ten, doi gia hoac xoa sau nay.

## 4.15. Tracking Don Hang

Co 2 dau ra:

1. MVC view: `GET /Order/Tracking/{id}`
2. API: `GET /api/orders/{id}/tracking`
3. MVC lookup: `GET /Order/Lookup`

Luong:

1. Service `GetTrackingAsync(orderId, userId)` load order cua user.
2. Tra ra status, payment info, shipping carrier, tracking number, tracking URL, cac moc ship/deliver.
3. Voi guest order o session moi hoac trinh duyet moi, user co the bat dau tu `Lookup`, nhap `OrderNumber` + `Email`, sau do moi duoc redirect vao `Details`/`Tracking`.

Tai sao tach tracking thanh mot view rieng:

1. User co the xem nhanh hanh trinh don ma khong can doc toan bo chi tiet order.

## 4.16. Huy Don, Hoan Ton, Refund-ready

Thanh phan:

1. `OrderService.CancelAsync(...)`
2. `OrderService.CancelOrderInternalAsync(...)`
3. `CanBeCancelled(order)`

Luong nghiep vu:

1. Load order kem items va status history.
2. Kiem tra don con duoc huy khong:
   - chua cancelled
   - chua completed
   - chua ship
3. Neu ton kho chua hoan, cong lai ton kho cho tung product.
4. Dat `IsInventoryRestored = true`.
5. Dat trang thai order = `Cancelled`.
6. Xoa tracking info.
7. Luu ly do huy va ai huy.
8. Neu don da `Paid` hoac `Authorized`, dat `IsRefundReady = true`.
9. Ghi `OrderStatusHistory`.
10. Save DB.
11. Gui thong bao realtime.

Tai sao can `IsInventoryRestored`:

1. Tranh cong ton kho nhieu lan neu mot flow cancel chay lap.

Tai sao can `IsRefundReady`:

1. Tach viec huy don khoi viec xu ly hoan tien thuc te.
2. He thong co the mo rong sau nay voi quy trinh refund rieng.

## 4.17. Quan Tri Don Hang

Thanh phan:

1. `Admin/OrdersController`
2. `OrderService.GetAdminOrderListAsync(...)`
3. `OrderService.GetAdminDetailsAsync(...)`
4. `OrderService.UpdateStatusAsync(...)`

### Admin list

1. Ho tro search theo `OrderNumber`, `ShippingFullName`, `ShippingEmail`.
2. Ho tro loc theo `OrderStatus`.
3. Projection ra `OrderAdminListItemViewModel`.

### Admin details

1. Load order + items + history.
2. Tao `OrderAdminDetailsViewModel`.
3. Tao `StatusForm` mac dinh de admin cap nhat.

### Admin update status

1. Controller kiem tra `id == model.OrderId`.
2. Neu invalid, load lai details.
3. Service chuan hoa input shipping/tracking/note.
4. Neu status moi la `Cancelled`, goi flow cancel trung tam.
5. Neu khong:
   - chan dua don da cancelled quay lai workflow active
   - cap nhat status neu thay doi
   - cap nhat tracking data neu thay doi
   - neu vao `Processing` ma chua ship, set `ShippedAtUtc`
   - neu vao `Completed`, set `DeliveredAtUtc`
   - neu COD va payment pending, chuyen `PaymentStatus` sang `Paid`
   - clear refund-ready flag neu can
6. Ghi lich su neu co thay doi.
7. Save DB.
8. Gui realtime notification.

Tai sao admin flow co nhieu rang buoc hon user flow:

1. Day la noi co the thay doi toan bo trang thai don hang.
2. Sai o day anh huong den ton kho, refund va tracking.

## 4.18. Ho So Nguoi Dung

Thanh phan:

1. `ProfileController`
2. `AccountService`

### Hien thi profile

1. `GetProfilePageAsync(userId)` load user theo id.
2. Lay roles cua user.
3. Tao `ProfilePageViewModel`, trong do `ProfileForm` duoc prefill du lieu hien tai.

### Cap nhat profile

1. Controller nhan `ProfileEditViewModel`.
2. Neu invalid, load lai `ProfilePageViewModel` va gan lai form.
3. `AccountService.UpdateProfileAsync(...)`:
   - load user
   - kiem tra email trung voi user khac khong
   - trim va cap nhat thong tin ca nhan
   - cap nhat default address
   - dat `EmailConfirmed = true`
   - `_userManager.UpdateAsync(user)`
   - `_signInManager.RefreshSignInAsync(user)`

### Doi mat khau

1. Controller nhan `ChangePasswordViewModel`.
2. Neu invalid, load lai page model.
3. `AccountService.ChangePasswordAsync(...)` goi `_userManager.ChangePasswordAsync(...)`.
4. Refresh sign-in.

Tai sao phai `RefreshSignInAsync` sau update profile va doi password:

1. De auth cookie hien tai dong bo voi thong tin moi nhat cua user.

## 4.19. Dashboard Admin

Thanh phan:

1. `DashboardController`
2. `AdminDashboardService.GetAsync()`

Du lieu tong hop:

1. dem user theo role
2. tong so product
3. tong so product active
4. tong so order
5. pending orders
6. pending reviews
7. total revenue
8. recent orders
9. low stock products

Tai sao dashboard duoc tong hop trong service:

1. Query tong hop phuc tap hon controller nen dat trong service de de bao tri.

## 4.20. Quan Tri Nguoi Dung

Thanh phan:

1. `UsersController`
2. `UserAdminService`

### Danh sach user

1. Search theo thong tin co ban.
2. Loc theo role.
3. Dem so don cua tung user.

### Chi tiet user

1. Hien profile, role, thong ke order.
2. Tao `UpdateForm` de admin cap nhat.

### Cap nhat user

Luong:

1. Kiem tra `SelectedRole` hop le.
2. Load user.
3. Lay role hien tai.
4. Chan admin tu go quyen admin cua chinh minh.
5. Chan admin tu khoa tai khoan cua chinh minh.
6. Bao dam he thong van con it nhat mot admin.
7. Kiem tra email trung.
8. Update thong tin co ban.
9. Update lockout / email confirmed.
10. Neu role thay doi, bo role cu va them role moi.

Tai sao phai bao dam con it nhat mot admin:

1. Neu khong, he thong se co admin area nhung khong con ai quan tri duoc.

## 4.21. Cau Hinh He Thong

Thanh phan:

1. `SettingsController`
2. `SystemSettingService`
3. `Admin/SliderController`
4. `SliderService`

### Doc settings

1. `GetForEditAsync()` lay row duy nhat va map sang `SettingFormViewModel`.
2. `GetPublicAsync()` lay projection public de layout/home/checkout dung.

### Cap nhat settings

1. Controller nhan `SettingFormViewModel`.
2. Service load entity hien tai.
3. Trim va cap nhat text field.
4. Ghi pricing, promo, SMTP, SEO, culture va cac co `ShowHomepage*`.
5. Truong `SmtpPassword` chi ghi de len khi admin nhap gia tri moi; de trong se giu password hien tai.
6. Neu chon xoa logo, clear `LogoImagePath`.
7. Neu co file logo moi:
   - validate extension va size
   - luu vao `wwwroot/uploads/settings`
   - cap nhat `LogoImagePath`
8. Save DB.
9. Xoa file logo cu neu can.

Tai sao settings duoc tach public va edit:

1. Giao dien public chi can mot phan du lieu.
2. Admin edit can day du field.
3. Projection rieng giup tranh phoi bay du lieu khong can thiet.

### Quan tri slider

1. `Admin/SliderController` cung cap `Index`, `Create`, `Edit`, `Delete`.
2. `SliderService` la noi chiu trach nhiem validation, normalize URL, luu file anh nen va xoa file cu khi can.
3. Banner va regular slide dung cung mot entity `SliderItem` nhung validate theo `SliderItemType`.

Rang buoc chinh:

1. `Banner` duoc phep co `BackgroundImagePath`, nhung neu khong co thi storefront dung background mac dinh.
2. `Banner` can day du noi dung de render title/description/button.
3. `RegularSlide` bat buoc co `BackgroundImagePath`.
4. `RegularSlide.ClickUrl` la tuy chon, khong con bat buoc.
5. Moi slide co `IsActive` de admin an/hien ma khong can xoa.

## 4.22. Localization

Thanh phan:

1. `LanguageController.Switch(...)`
2. `CookieRequestCultureProvider`
3. `.resx`

Luong:

1. Nhan `culture` va `returnUrl`.
2. Chuan hoa culture chi trong `en`, `vi`.
3. Ghi cookie culture voi han 1 nam.
4. Redirect lai trang cu neu local URL.

Tai sao can `returnUrl`:

1. User doi ngon ngu o bat ky trang nao cung nen duoc quay lai dung trang do.

## 4.23. Realtime Notification

Thanh phan:

1. `NotificationHub`
2. `RealtimeNotificationService`
3. `wwwroot/js/site.js`
4. `_Layout.cshtml`

### Khi client ket noi

1. Layout nap script.
2. `site.js` mo connection toi `/hubs/notifications` neu user dang nhap.
3. `NotificationHub.OnConnectedAsync()` lay `userId`.
4. Add connection vao group user.
5. Neu la admin, add vao group admin.

### Khi tao don

1. `OrderService.CreateOrderAsync(...)` goi `NotifyOrderCreatedAsync(...)`.
2. Service gui event den user group va admin group.
3. Client nhan event va render toast.

### Khi cap nhat don

1. `UpdateStatusAsync(...)` hoac `CancelAsync(...)` goi `NotifyOrderUpdatedAsync(...)`.
2. Client hien thi toast moi.

Tai sao chia group theo user va admin:

1. User chi can biet don cua minh.
2. Admin can biet toan bo don moi / cap nhat lien quan quan tri.

## 4.24. API

## 4.24.1. Catalog API

Endpoint:

1. `GET /api/catalog/categories`
2. `GET /api/catalog/products`
3. `GET /api/catalog/products/{id}`

Tai sao API catalog chi tra du lieu active:

1. API nay la public storefront API, khong phai admin API.

Du lieu gia trong API:

1. `ProductSummaryDto` tra `EffectivePrice`.
2. DTO cung tra `IsDiscountActive` va `DiscountPercentage`.
3. Frontend hoac consumer API co the biet gia dang ban va trang thai giam gia ma khong phai tu tinh lai.

## 4.24.2. Storefront settings API

Endpoint:

1. `GET /api/storefront/settings`

Luong:

1. `StorefrontController` goi `_systemSettingService.GetPublicAsync()`.
2. Tra JSON settings public.

## 4.24.3. Orders API

Endpoint:

1. `GET /api/orders`
2. `GET /api/orders/{id}/tracking`

Rang buoc:

1. `[Authorize]`

Luong:

1. Lay `userId` tu claims.
2. Neu khong co, `Unauthorized()`.
3. `GetOrders` doc lich su don cua user hien tai.
4. `GetTracking` doc tracking don cua user hien tai.

Tai sao API orders khong cho truyen userId tu query:

1. Tranh lo du lieu cua user khac.
2. User hien tai duoc xac dinh bang auth claims, khong phai input client.

## 4.25. Shared Layout va Du Lieu Toan Cuc

File trung tam:

1. `Views/Shared/_Layout.cshtml`

Service inject:

1. `ICartService`
2. `ISystemSettingService`
3. `IWishlistService`
4. `IStringLocalizer<SharedResource>`

Bien duoc tinh ngay dau layout:

1. `cartItemCount = await CartService.GetItemCountAsync()`
2. `siteSettings = await SystemSettingService.GetPublicAsync()`
3. `adminLandingController`
4. `currentUiCulture`
5. `returnUrl`
6. `wishlistItemCount`

Luong render shell storefront:

1. Moi request vao mot Razor view dung layout nay deu chay khoi code Razor o dau layout truoc.
2. Layout doc so luong gio hang tu session qua `CartService.GetItemCountAsync()`.
3. Layout doc settings public de render ten shop, logo, title, meta description.
4. Neu user login, layout doc so luong wishlist tu DB qua `WishlistService.GetItemCountAsync(userId)`.
5. Layout render nav, account menu, language switch, theme toggle, notification host.

Tai sao layout co the cap nhat du lieu toan cuc sau moi redirect:

1. Sau moi `Redirect`, trinh duyet tao request moi.
2. Request moi lai render layout tu dau.
3. Vi layout tu doc service o moi request, shell luon lay state moi nhat cua cart, wishlist va settings.

Tai sao cart count va wishlist count duoc lay bang 2 cach khac nhau:

1. Cart count doc session vi gio hang nam trong session.
2. Wishlist count doc DB vi wishlist gan voi user account.

Tai sao user chua login van thay gio hang nhung khong co wishlist:

1. Session khong can account.
2. Wishlist can `userId`.

## 5. Rang Buoc Du Lieu Va Ky Thuat Quan Trong

## 5.1. Rang buoc du lieu

1. Ten category la duy nhat.
2. Review unique theo `(ProductId, UserId)`.
3. Wishlist unique theo `(UserId, ProductId)`.
4. `OrderNumber` la duy nhat.
5. Enum duoc luu dang string trong DB.

Tai sao quan trong:

1. Bao toan ven du lieu.
2. De query, audit va debug.

## 5.2. Rang buoc nghiep vu

1. Product phai `Active` moi hien storefront.
2. Chi user co don `Completed` moi duoc review.
3. Review moi vao `Pending` truoc khi public.
4. Category con product thi khong duoc xoa.
5. Don da ship/completed thi khong cho customer huy.
6. He thong phai con it nhat mot admin hop le.
7. Homepage section chi render khi co `ShowHomepage* = true`.
8. Slider chi render item `IsActive = true`.
9. Direct discount chi co hieu luc khi `IsDiscountActive = true` va `DiscountPercentage > 0`.

## 5.3. Rang buoc upload

1. Anh product:
   - `.jpg`, `.jpeg`, `.png`, `.webp`
   - toi da 5MB
2. Logo settings:
   - `.jpg`, `.jpeg`, `.png`, `.webp`, `.svg`
   - toi da 2MB
3. Anh slider background:
   - `.jpg`, `.jpeg`, `.png`, `.webp`
   - validate trong `SliderService`

Tai sao validate upload o service:

1. Controller khong nen giu logic file validation phuc tap.
2. Service co the tai su dung va de test hon.

## 5.4. Rang buoc discount va pricing

1. Gia hieu luc duoc tinh tap trung trong `ProductPricing`.
2. `DiscountPercentage` phai nam trong khoang hop le de khong tao ra gia am.
3. Gio hang, checkout va order snapshot khong tu tinh lai theo cach rieng.
4. Promo code toan cuc trong `SystemSetting` va direct discount tren `Product` la hai lop tinh gia khac nhau:
   - direct discount giam gia tung san pham
   - promo code giam tren subtotal checkout

## 5.5. Cookie va Session

1. Auth cookie: `ASPNET_Ecommerce.Auth`
2. Session cookie: `ASPNET_Ecommerce.Session`
3. Cart key: `cart.items`
4. Culture cookie: mac dinh cua `CookieRequestCultureProvider`

Tai sao phai ghi ro 4 gia tri nay:

1. Day la cac diem quan trong khi debug login, gio hang va chuyen ngon ngu.

## 6. Quan He Du Lieu Chinh

1. `ApplicationUser -> Orders`: 1-n
2. `Order -> OrderItems`: 1-n
3. `Order -> OrderStatusHistory`: 1-n
4. `Category -> Products`: 1-n
5. `Product -> ProductImages`: 1-n
6. `Product -> ProductReviews`: 1-n
7. `ApplicationUser -> ProductReview`: 1-n
8. `ApplicationUser -> WishlistItem`: 1-n
9. `Product -> WishlistItem`: 1-n
10. `SystemSetting -> SliderItems`: quan he logic 1-n theo cung mot storefront

Tai sao quan he nay can duoc hieu ro:

1. No giai thich vi sao mot action detail product phai include images, review, category.
2. No giai thich vi sao xoa category khong the tu do khi van con products.
3. No giai thich vi sao order details can load items va history rieng.

## 7. Tom Tat Vai Tro Tung Nhom File

1. `Models/`
   - dinh nghia du lieu nghiep vu, enum, quan he.

2. `Models/ViewModels/`
   - dinh nghia du lieu input/output cho man hinh.

3. `Controllers/`
   - nhan request, authorize, validate co ban, redirect va tra view/json.

4. `Services/`
   - noi xu ly nghiep vu trung tam.

5. `Data/`
   - noi dinh nghia schema va khoi tao du lieu nen tang.

6. `Resources/`
   - noi chua chuoi da ngon ngu.

7. `Views/`
   - noi render giao dien.

8. `Hubs/` + `wwwroot/js/site.js`
   - noi xu ly realtime va hanh vi giao dien dung chung.

## 8. Ket Luan

ASPNET Ecommerce hien tai la mot he thong MVC thuong mai dien tu co cau truc kha ro rang:

1. Controller mong.
2. Service giu logic nghiep vu.
3. DB luu du lieu ben vung nhu user, wishlist, order, review, settings.
4. Session luu du lieu tam thoi cua gio hang.
5. Layout doc du lieu toan cuc de giu shell storefront dong bo.
6. Admin area tach ro voi public area.
7. Pricing, checkout, order status, review management va user administration deu co service trung tam.

Neu can tom gon bang ngon ngu kien truc:

1. Storefront UI doc du lieu qua service.
2. User thao tac qua controller.
3. Service dung `ApplicationDbContext`, session, Identity va SignalR de xu ly.
4. Ket qua cuoi cung duoc phan anh lai o view, badge navbar, trang chi tiet, lich su don va admin dashboard.

Noi cach khac, day la mot he thong co mot nguon su that rieng cho tung loai du lieu:

1. Cart -> session
2. Wishlist -> database theo user
3. Auth -> Identity cookie + AspNet tables
4. Order -> database + status history
5. UI shell -> layout doc lai state moi request

Chinh viec tach ro cac nguon su that nay la ly do he thong co the giu duoc tinh dong bo giua giao dien, ton kho, thanh toan, wishlist, cart va order flow.
