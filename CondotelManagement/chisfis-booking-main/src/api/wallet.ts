import axiosClient from "./axiosClient";

// Wallet DTOs
export interface WalletDTO {
  walletId: number;
  hostId: number;
  bankName: string; // Tên ngân hàng
  bankCode?: string; // Mã ngân hàng (MB, VCB, TCB, etc.)
  accountNumber: string; // Số tài khoản
  accountHolderName: string; // Tên chủ tài khoản
  isDefault: boolean; // Có phải tài khoản mặc định không
  createdAt?: string;
  updatedAt?: string;
}

export interface WalletCreateDTO {
  bankName: string;
  bankCode?: string;
  accountNumber: string;
  accountHolderName: string;
  hostId?: number; // Sẽ được set tự động từ token
}

export interface WalletUpdateDTO {
  bankName?: string;
  bankCode?: string;
  accountNumber?: string;
  accountHolderName?: string;
}

// Helper function to normalize wallet data
const normalizeWallet = (item: any): WalletDTO => ({
  walletId: item.WalletId || item.walletId,
  hostId: item.HostId || item.hostId,
  bankName: item.BankName || item.bankName,
  bankCode: item.BankCode || item.bankCode,
  accountNumber: item.AccountNumber || item.accountNumber,
  accountHolderName: item.AccountHolderName || item.accountHolderName,
  isDefault: item.IsDefault !== undefined ? item.IsDefault : (item.isDefault !== undefined ? item.isDefault : false),
  createdAt: item.CreatedAt || item.createdAt,
  updatedAt: item.UpdatedAt || item.updatedAt,
});

// API Calls
export const walletAPI = {
  // GET /api/host/wallet - Lấy danh sách tài khoản ngân hàng của host
  getAll: async (): Promise<WalletDTO[]> => {
    const response = await axiosClient.get<any>("/host/wallet");
    const data = response.data;
    
    // Backend trả về { success, data, total }
    const wallets = data.success && data.data ? data.data : (Array.isArray(data) ? data : []);
    
    return wallets.map(normalizeWallet);
  },

  // POST /api/host/wallet - Tạo tài khoản ngân hàng mới
  create: async (dto: WalletCreateDTO): Promise<WalletDTO> => {
    const response = await axiosClient.post<any>("/host/wallet", dto);
    const data = response.data;
    
    // Backend trả về { success, message, data }
    const wallet = data.success && data.data ? data.data : data;
    
    return normalizeWallet(wallet);
  },

  // PUT /api/host/wallet/{id} - Cập nhật tài khoản ngân hàng
  update: async (id: number, dto: WalletUpdateDTO): Promise<void> => {
    await axiosClient.put(`/host/wallet/${id}`, dto);
  },

  // DELETE /api/host/wallet/{id} - Xóa tài khoản ngân hàng
  delete: async (id: number): Promise<void> => {
    await axiosClient.delete(`/host/wallet/${id}`);
  },

  // POST /api/host/wallet/{id}/set-default - Đặt tài khoản ngân hàng làm mặc định
  setDefault: async (id: number): Promise<void> => {
    await axiosClient.post(`/host/wallet/${id}/set-default`);
  },
};

export default walletAPI;




