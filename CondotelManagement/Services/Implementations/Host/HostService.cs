using CondotelManagement.Data;
using CondotelManagement.DTOs;
using CondotelManagement.DTOs.Host;
using CondotelManagement.Models;
using CondotelManagement.Repositories;
using CondotelManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using HostModel = CondotelManagement.Models.Host;

// Sử dụng tiền tố đầy đủ để tránh lỗi "ambiguous reference" và loại bỏ 'using CondotelManagement.Models;'

namespace CondotelManagement.Services
{
    public class HostService : IHostService
    {
        private readonly IHostRepository _hostRepo;
        private readonly CondotelDbVer1Context _context;

        // SỬA LỖI: Chỉ giữ lại CondotelDbVer1Context vì các repo đã bị xóa
        public HostService(CondotelDbVer1Context context, IHostRepository hostRepo)
        {
            _context = context;
            _hostRepo = hostRepo;
        }

        public async Task<HostRegistrationResponseDto> RegisterHostAsync(int userId, HostRegisterRequestDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new Exception("Không tìm thấy người dùng (UserID từ Token không tồn tại).");
            }

            // 2. Tìm Host và Wallet hiện tại
            var existingHost = await _context.Hosts
                .Include(h => h.Wallet)
                .FirstOrDefaultAsync(h => h.UserId == userId);

            // --- Khai báo biến cần thiết ---
            HostModel hostToProcess;
            Wallet walletToProcess = null;
            bool isNewHost = existingHost == null;
            bool isNewWallet = false;

            // --- LOGIC: USER MỚI HOÀN TOÀN (existingHost == null) ---
            if (isNewHost)
            {
                // 3. Tạo bản ghi Host mới
                hostToProcess = new HostModel
                {
                    UserId = userId,
                    PhoneContact = dto.PhoneContact,
                    Address = dto.Address,
                    CompanyName = dto.CompanyName,
                    Status = "Active",
                    // ❌ FIX: Đã XÓA 'RegistrationDate = DateTime.UtcNow'
                    //Wallets = new System.Collections.Generic.List<CondotelManagement.Models.Wallet>()
                };
                _context.Hosts.Add(hostToProcess);

                // 4. Tạo Wallet mới
                walletToProcess = new Wallet
                {
                    // FIX: BỎ UserId để thỏa mãn CK_Wallet_OneOwner (Wallet chỉ có 1 FK)
                    BankName = dto.BankName,
                    AccountNumber = dto.AccountNumber,
                    AccountHolderName = dto.AccountHolderName
                };

                isNewWallet = true;

                // 5. Nâng cấp quyền    
                user.RoleId = 3;
            }
            // --- LOGIC: USER ĐÃ LÀ HOST (existingHost != null) ---
            else
            {
                hostToProcess = existingHost;

                // Cập nhật thông tin Host
                hostToProcess.PhoneContact = dto.PhoneContact;
                hostToProcess.Address = dto.Address;
                hostToProcess.CompanyName = dto.CompanyName;

                var existingWallet = existingHost.Wallet;

                if (existingWallet != null)
                {
                    // UPDATE Wallet
                    walletToProcess = existingWallet;
                    walletToProcess.BankName = dto.BankName;
                    walletToProcess.AccountNumber = dto.AccountNumber;
                    walletToProcess.AccountHolderName = dto.AccountHolderName;
                }
                else
                {
                    // TẠO WALLET MỚI nếu Host đã có nhưng Wallet bị thiếu/inActive
                    walletToProcess = new Wallet
                    {
                        // FIX: BỎ UserId để thỏa mãn CK_Wallet_OneOwner (Wallet chỉ có 1 FK)
                        HostId = existingHost.HostId,
                        BankName = dto.BankName,
                        AccountNumber = dto.AccountNumber,
                        AccountHolderName = dto.AccountHolderName
                    };
                    isNewWallet = true;
                }
            }

            // 6. THỰC HIỆN THAO TÁC DB CHUNG: ADD WALLET
            if (isNewWallet && walletToProcess != null)
            {
                if (isNewHost)
                {
                    // Gán vào Host Collection để EF Core xử lý chuỗi Insert (Host -> Wallet)
                    //hostToProcess.Wallets.Add(walletToProcess);
                }
                else
                {
                    // Host cũ đã có HostId, chỉ cần Add Wallet vào DbContext
                    _context.Wallets.Add(walletToProcess);
                }
            }

            // 7. COMMIT TẤT CẢ
            // Nếu bạn có vấn đề về update RoleId, bạn có thể tách SaveChangesAsync thành 2 lần
            // Nhưng hiện tại giữ nguyên 1 lần commit là hiệu quả nhất.
            await _context.SaveChangesAsync();

            if (isNewHost && isNewWallet && walletToProcess != null)
            {
                walletToProcess.HostId = hostToProcess.HostId;  // ← Cứu cả thế giới
                _context.Wallets.Add(walletToProcess);
            }

            // 8. LƯU LẦN CUỐI
            await _context.SaveChangesAsync();

            // 9. TRẢ VỀ
            return new HostRegistrationResponseDto
            {
                HostId = hostToProcess.HostId,
                Message = isNewHost ? "Đăng ký làm Host thành công!" : "Cập nhật thông tin thành công!"
            };
        }

        public HostModel GetByUserId(int userId)
        {
            return _hostRepo.GetByUserId(userId);
        }

        public Task<bool> CanHostUploadCondotel(int hostId)
        {
            throw new NotImplementedException();
        }

        public async Task<HostProfileDTO> GetHostProfileAsync(int userId)
        {
            var host = await _hostRepo.GetHostProfileAsync(userId);
            if (host == null) return null;

            return new HostProfileDTO
            {
                HostID = host.HostId,
                CompanyName = host.CompanyName,
                Address = host.Address,
                PhoneContact = host.PhoneContact,

                UserID = host.User.UserId,
                FullName = host.User.FullName,
                Email = host.User.Email,
                Phone = host.User.Phone,
                Gender = host.User.Gender,
                DateOfBirth = host.User.DateOfBirth,
                UserAddress = host.User.Address,
                ImageUrl = host.User.ImageUrl,

                Packages = host.HostPackages.Select(hp => new HostPackageDTO
                {
                    PackageID = hp.PackageId,
                    Name = hp.Package.Name,
                    StartDate = hp.StartDate,
                    EndDate = hp.EndDate
                }).ToList(),

                Wallet = new WalletDTO
                {
                    WalletID = host.Wallet.WalletId,
                    BankName = host.Wallet.BankName,
                    AccountNumber = host.Wallet.AccountNumber,
                    AccountHolderName = host.Wallet.AccountHolderName
                }
            };
        }

        public async Task<bool> UpdateHostProfileAsync(int userId, UpdateHostProfileDTO dto)
        {
            var host = await _hostRepo.GetHostProfileAsync(userId);
            if (host == null) return false;

            // Update HOST
            host.CompanyName = dto.CompanyName;
            host.Address = dto.Address;
            host.PhoneContact = dto.PhoneContact;

            // Update USER
            host.User.FullName = dto.FullName;
            host.User.Phone = dto.Phone;
            host.User.Gender = dto.Gender;
            host.User.DateOfBirth = dto.DateOfBirth;
            host.User.Address = dto.UserAddress;
            host.User.ImageUrl = dto.ImageUrl;

            // Update Wallet
            host.Wallet.BankName = dto.BankName;
            host.Wallet.AccountNumber = dto.AccountNumber;
            host.Wallet.AccountHolderName = dto.AccountHolderName;

            await _hostRepo.UpdateHostAsync(host);
            return true;
        }
    }
}