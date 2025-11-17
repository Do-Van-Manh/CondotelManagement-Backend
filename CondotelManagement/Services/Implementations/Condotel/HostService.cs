using CondotelManagement.Data;
using CondotelManagement.DTOs;
using CondotelManagement.DTOs.Host;
using CondotelManagement.Models;
using CondotelManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

// Sử dụng tiền tố đầy đủ để tránh lỗi "ambiguous reference" và loại bỏ 'using CondotelManagement.Models;'

namespace CondotelManagement.Services.Implementations
{
    public class HostService : IHostService
    {
        private readonly CondotelDbVer1Context _context;

        // SỬA LỖI: Chỉ giữ lại CondotelDbVer1Context vì các repo đã bị xóa
        public HostService(CondotelDbVer1Context context)
        {
            _context = context;
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
                .Include(h => h.Wallets)
                .FirstOrDefaultAsync(h => h.UserId == userId);

            // --- Khai báo biến cần thiết ---
            CondotelManagement.Models.Host hostToProcess;
            CondotelManagement.Models.Wallet walletToProcess = null;
            bool isNewHost = existingHost == null;
            bool isNewWallet = false;

            // --- LOGIC: USER MỚI HOÀN TOÀN (existingHost == null) ---
            if (isNewHost)
            {
                // 3. Tạo bản ghi Host mới
                hostToProcess = new CondotelManagement.Models.Host
                {
                    UserId = userId,
                    PhoneContact = dto.PhoneContact,
                    Address = dto.Address,
                    CompanyName = dto.CompanyName,
                    Status = "Active",
                    // ❌ FIX: Đã XÓA 'RegistrationDate = DateTime.UtcNow'
                    Wallets = new System.Collections.Generic.List<CondotelManagement.Models.Wallet>()
                };
                _context.Hosts.Add(hostToProcess);

                // 4. Tạo Wallet mới
                walletToProcess = new CondotelManagement.Models.Wallet
                {
                    // FIX: BỎ UserId để thỏa mãn CK_Wallet_OneOwner (Wallet chỉ có 1 FK)
                    BankName = dto.BankName,
                    AccountNumber = dto.AccountNumber,
                    AccountHolderName = dto.AccountHolderName,
                    Status = "Active",
                };

                isNewWallet = true;

                // 5. Nâng cấp quyền    
                user.RoleId = 4;
            }
            // --- LOGIC: USER ĐÃ LÀ HOST (existingHost != null) ---
            else
            {
                hostToProcess = existingHost;

                // Cập nhật thông tin Host
                hostToProcess.PhoneContact = dto.PhoneContact;
                hostToProcess.Address = dto.Address;
                hostToProcess.CompanyName = dto.CompanyName;

                var existingWallet = existingHost.Wallets?.FirstOrDefault(w => w.Status == "Active");

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
                    walletToProcess = new CondotelManagement.Models.Wallet
                    {
                        // FIX: BỎ UserId để thỏa mãn CK_Wallet_OneOwner (Wallet chỉ có 1 FK)
                        HostId = existingHost.HostId,
                        BankName = dto.BankName,
                        AccountNumber = dto.AccountNumber,
                        AccountHolderName = dto.AccountHolderName,
                        Status = "Active",
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
                    hostToProcess.Wallets.Add(walletToProcess);
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

            // 8. TRẢ VỀ DTO
            return new HostRegistrationResponseDto
            {
                HostId = hostToProcess.HostId
            };
        }

        public CondotelManagement.Models.Host? GetByUserId(int userId)
        {
            return _context.Hosts.FirstOrDefault(h => h.UserId == userId);
        }

        public Task<bool> CanHostUploadCondotel(int hostId)
        {
            throw new NotImplementedException();
        }

        public Task<HostProfileDTO?> GetHostProfileAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateHostProfileAsync(int userId, UpdateHostProfileDTO dto)
        {
            throw new NotImplementedException();
        }
    }
}