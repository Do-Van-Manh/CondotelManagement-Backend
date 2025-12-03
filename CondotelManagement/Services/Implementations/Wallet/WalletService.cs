using CondotelManagement.Data;
using CondotelManagement.DTOs.Wallet;
using CondotelManagement.Models;
using CondotelManagement.Services.Interfaces.Wallet;
using Microsoft.EntityFrameworkCore;

namespace CondotelManagement.Services.Implementations.Wallet
{
    public class WalletService : IWalletService
    {
        private readonly CondotelDbVer1Context _context;

        public WalletService(CondotelDbVer1Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WalletDTO>> GetWalletsByUserIdAsync(int userId)
        {
            var wallets = await _context.Wallets
                .Where(w => w.UserId == userId)
                .ToListAsync();

            return wallets.Select(w => new WalletDTO
            {
                WalletId = w.WalletId,
                UserId = w.UserId,
                HostId = w.HostId,
                BankName = w.BankName,
                AccountNumber = w.AccountNumber,
                AccountHolderName = w.AccountHolderName,
                Status = w.Status,
                IsDefault = w.IsDefault
            });
        }

        public async Task<IEnumerable<WalletDTO>> GetWalletsByHostIdAsync(int hostId)
        {
            var wallets = await _context.Wallets
                .Where(w => w.HostId == hostId)
                .ToListAsync();

            return wallets.Select(w => new WalletDTO
            {
                WalletId = w.WalletId,
                UserId = w.UserId,
                HostId = w.HostId,
                BankName = w.BankName,
                AccountNumber = w.AccountNumber,
                AccountHolderName = w.AccountHolderName,
                Status = w.Status,
                IsDefault = w.IsDefault
            });
        }

        public async Task<WalletDTO?> GetWalletByIdAsync(int walletId)
        {
            var wallet = await _context.Wallets.FindAsync(walletId);
            if (wallet == null) return null;

            return new WalletDTO
            {
                WalletId = wallet.WalletId,
                UserId = wallet.UserId,
                HostId = wallet.HostId,
                BankName = wallet.BankName,
                AccountNumber = wallet.AccountNumber,
                AccountHolderName = wallet.AccountHolderName,
                Status = wallet.Status,
                IsDefault = wallet.IsDefault
            };
        }

        public async Task<WalletDTO> CreateWalletAsync(WalletCreateDTO dto)
        {
            // Nếu set làm default, bỏ default của các wallet khác
            if (dto.IsDefault)
            {
                if (dto.UserId.HasValue)
                {
                    var userWallets = await _context.Wallets
                        .Where(w => w.UserId == dto.UserId && w.IsDefault)
                        .ToListAsync();
                    foreach (var w in userWallets)
                    {
                        w.IsDefault = false;
                    }
                }
                else if (dto.HostId.HasValue)
                {
                    var hostWallets = await _context.Wallets
                        .Where(w => w.HostId == dto.HostId && w.IsDefault)
                        .ToListAsync();
                    foreach (var w in hostWallets)
                    {
                        w.IsDefault = false;
                    }
                }
            }

            var wallet = new Models.Wallet
            {
                UserId = dto.UserId,
                HostId = dto.HostId,
                BankName = dto.BankName,
                AccountNumber = dto.AccountNumber,
                AccountHolderName = dto.AccountHolderName,
                Status = "Active",
                IsDefault = dto.IsDefault
            };

            _context.Wallets.Add(wallet);
            await _context.SaveChangesAsync();

            return new WalletDTO
            {
                WalletId = wallet.WalletId,
                UserId = wallet.UserId,
                HostId = wallet.HostId,
                BankName = wallet.BankName,
                AccountNumber = wallet.AccountNumber,
                AccountHolderName = wallet.AccountHolderName,
                Status = wallet.Status,
                IsDefault = wallet.IsDefault
            };
        }

        public async Task<bool> UpdateWalletAsync(int walletId, WalletUpdateDTO dto)
        {
            var wallet = await _context.Wallets.FindAsync(walletId);
            if (wallet == null) return false;

            if (!string.IsNullOrEmpty(dto.BankName))
                wallet.BankName = dto.BankName;
            
            if (!string.IsNullOrEmpty(dto.AccountNumber))
                wallet.AccountNumber = dto.AccountNumber;
            
            if (!string.IsNullOrEmpty(dto.AccountHolderName))
                wallet.AccountHolderName = dto.AccountHolderName;
            
            if (!string.IsNullOrEmpty(dto.Status))
                wallet.Status = dto.Status;
            
            if (dto.IsDefault.HasValue)
            {
                wallet.IsDefault = dto.IsDefault.Value;
                
                // Nếu set làm default, bỏ default của các wallet khác
                if (dto.IsDefault.Value)
                {
                    if (wallet.UserId.HasValue)
                    {
                        var userWallets = await _context.Wallets
                            .Where(w => w.UserId == wallet.UserId && w.WalletId != walletId && w.IsDefault)
                            .ToListAsync();
                        foreach (var w in userWallets)
                        {
                            w.IsDefault = false;
                        }
                    }
                    else if (wallet.HostId.HasValue)
                    {
                        var hostWallets = await _context.Wallets
                            .Where(w => w.HostId == wallet.HostId && w.WalletId != walletId && w.IsDefault)
                            .ToListAsync();
                        foreach (var w in hostWallets)
                        {
                            w.IsDefault = false;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteWalletAsync(int walletId)
        {
            var wallet = await _context.Wallets.FindAsync(walletId);
            if (wallet == null) return false;

            _context.Wallets.Remove(wallet);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetDefaultWalletAsync(int walletId, int? userId = null, int? hostId = null)
        {
            var wallet = await _context.Wallets.FindAsync(walletId);
            if (wallet == null) return false;

            // Bỏ default của các wallet khác
            if (userId.HasValue)
            {
                var userWallets = await _context.Wallets
                    .Where(w => w.UserId == userId && w.WalletId != walletId)
                    .ToListAsync();
                foreach (var w in userWallets)
                {
                    w.IsDefault = false;
                }
            }
            else if (hostId.HasValue)
            {
                var hostWallets = await _context.Wallets
                    .Where(w => w.HostId == hostId && w.WalletId != walletId)
                    .ToListAsync();
                foreach (var w in hostWallets)
                {
                    w.IsDefault = false;
                }
            }

            wallet.IsDefault = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

