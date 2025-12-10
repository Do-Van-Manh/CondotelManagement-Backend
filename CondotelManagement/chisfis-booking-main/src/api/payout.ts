import axiosClient from "./axiosClient";

// HostPayoutDTO - Thông tin payout cho booking (theo DTO mới từ backend)
export interface HostPayoutDTO {
  bookingId: number;
  condotelId: number;
  condotelName?: string;
  hostId?: number; // ID của host
  hostName?: string; // Tên host
  customerId: number;
  customerName?: string;
  customerEmail?: string;
  startDate?: string; // DateOnly format: YYYY-MM-DD (optional trong DTO mới)
  endDate: string; // DateOnly format: YYYY-MM-DD
  amount: number; // Số tiền thanh toán (thay vì totalPrice)
  totalPrice?: number; // Giữ lại để backward compatibility
  status?: string; // "Pending", "Confirmed", "Cancelled", "Completed"
  promotionId?: number;
  isPaidToHost?: boolean; // Đã thanh toán chưa
  isPaid?: boolean; // Alias cho isPaidToHost
  paidToHostAt?: string; // DateTime
  paidAt?: string; // DateTime (alias)
  completedAt?: string; // DateTime (EndDate)
  daysSinceCompleted?: number; // Số ngày từ khi completed
  createdAt?: string; // DateTime
  
  // Thông tin ngân hàng của host (để admin chuyển tiền)
  bankName?: string; // Tên ngân hàng của host
  accountNumber?: string; // Số tài khoản ngân hàng của host
  accountHolderName?: string; // Tên chủ tài khoản của host
}

// ProcessPayoutResponse - Response khi xử lý payout (theo HostPayoutResponseDTO mới)
export interface ProcessPayoutResponse {
  success: boolean;
  message: string;
  processedCount?: number; // Số lượng đã xử lý (thay vì processedBookings)
  processedBookings?: number; // Giữ lại để backward compatibility
  totalAmount?: number;
  processedItems?: HostPayoutDTO[]; // Danh sách items đã xử lý (thay vì bookings)
  bookings?: HostPayoutDTO[]; // Giữ lại để backward compatibility
}

// Helper function to normalize payout data (theo HostPayoutItemDTO mới)
const normalizePayout = (item: any): HostPayoutDTO => {
  const amount = item.Amount !== undefined ? item.Amount : (item.amount !== undefined ? item.amount : (item.TotalPrice !== undefined ? item.TotalPrice : item.totalPrice || 0));
  const isPaid = item.IsPaid !== undefined ? item.IsPaid : (item.isPaid !== undefined ? item.isPaid : (item.IsPaidToHost !== undefined ? item.IsPaidToHost : item.isPaidToHost));
  
  return {
    bookingId: item.BookingId || item.bookingId,
    condotelId: item.CondotelId || item.condotelId,
    condotelName: item.CondotelName || item.condotelName,
    hostId: item.HostId !== undefined ? item.HostId : item.hostId,
    hostName: item.HostName || item.hostName,
    customerId: item.CustomerId || item.customerId,
    customerName: item.CustomerName || item.customerName,
    customerEmail: item.CustomerEmail || item.customerEmail,
    startDate: item.StartDate || item.startDate,
    endDate: item.EndDate || item.endDate,
    amount: amount,
    totalPrice: amount, // Giữ lại để backward compatibility
    status: item.Status || item.status,
    promotionId: item.PromotionId !== undefined ? item.PromotionId : item.promotionId,
    isPaidToHost: isPaid,
    isPaid: isPaid,
    paidToHostAt: item.PaidAt || item.paidAt || item.PaidToHostAt || item.paidToHostAt,
    paidAt: item.PaidAt || item.paidAt || item.PaidToHostAt || item.paidToHostAt,
    completedAt: item.CompletedAt || item.completedAt,
    daysSinceCompleted: item.DaysSinceCompleted !== undefined ? item.DaysSinceCompleted : item.daysSinceCompleted,
    createdAt: item.CreatedAt || item.createdAt,
    // Thông tin ngân hàng của host
    bankName: item.BankName || item.bankName,
    accountNumber: item.AccountNumber || item.accountNumber,
    accountHolderName: item.AccountHolderName || item.accountHolderName,
  };
};

// Helper function to normalize process response (theo HostPayoutResponseDTO mới)
const normalizeProcessResponse = (data: any): ProcessPayoutResponse => {
  const processedCount = data.ProcessedCount !== undefined ? data.ProcessedCount : (data.ProcessedBookings !== undefined ? data.ProcessedBookings : data.processedBookings);
  const processedItems = data.ProcessedItems || data.processedItems || data.Bookings || data.bookings || [];
  
  return {
    success: data.Success !== undefined ? data.Success : data.success !== undefined ? data.success : true,
    message: data.Message || data.message || "Đã xử lý payout thành công",
    processedCount: processedCount,
    processedBookings: processedCount, // Giữ lại để backward compatibility
    totalAmount: data.TotalAmount !== undefined ? data.TotalAmount : data.totalAmount,
    processedItems: processedItems.map(normalizePayout),
    bookings: processedItems.map(normalizePayout), // Giữ lại để backward compatibility
  };
};

// API Calls
export const payoutAPI = {
  // ========== HOST APIs ==========
  // GET /api/host/payout/pending - Lấy danh sách booking chờ thanh toán (Host chỉ xem được của mình)
  // Tự động lấy hostId từ token, không cần truyền
  getPendingPayouts: async (): Promise<HostPayoutDTO[]> => {
    const response = await axiosClient.get<any>("/host/payout/pending");
    const data = response.data;
    
    // Normalize response từ backend (PascalCase -> camelCase)
    const payouts = Array.isArray(data) ? data : (data.data || []);
    
    return payouts.map(normalizePayout);
  },

  // GET /api/host/payout/paid?fromDate=2025-01-01&toDate=2025-12-31 - Lấy danh sách booking đã thanh toán (Host chỉ xem được của mình)
  // Tự động lấy hostId từ token, không cần truyền
  // fromDate, toDate (optional): Lọc theo khoảng thời gian (YYYY-MM-DD)
  getPaidPayouts: async (options?: {
    fromDate?: string; // YYYY-MM-DD
    toDate?: string; // YYYY-MM-DD
  }): Promise<HostPayoutDTO[]> => {
    const params: any = {};
    if (options?.fromDate) {
      params.fromDate = options.fromDate;
    }
    if (options?.toDate) {
      params.toDate = options.toDate;
    }
    
    const response = await axiosClient.get<any>("/host/payout/paid", { params });
    const data = response.data;
    
    // Normalize response từ backend (PascalCase -> camelCase)
    const payouts = Array.isArray(data) ? data : (data.data || []);
    
    return payouts.map(normalizePayout);
  },

  // POST /api/host/payout/process/{bookingId} - Host request xử lý payout cho một booking cụ thể
  processPayout: async (bookingId: number): Promise<ProcessPayoutResponse> => {
    const response = await axiosClient.post<any>(`/host/payout/process/${bookingId}`);
    return normalizeProcessResponse(response.data);
  },

  // ========== ADMIN APIs ==========
  // GET /api/admin/payouts/pending?hostId=1 - Admin xem danh sách booking chờ thanh toán (tất cả hosts)
  // hostId (optional): Lọc theo host cụ thể, nếu null thì lấy tất cả
  getAdminPendingPayouts: async (hostId?: number): Promise<HostPayoutDTO[]> => {
    const params: any = {};
    if (hostId !== undefined && hostId !== null) {
      params.hostId = hostId;
    }
    
    const response = await axiosClient.get<any>("/admin/payouts/pending", { params });
    const data = response.data;
    
    // Normalize response từ backend (PascalCase -> camelCase)
    const payouts = Array.isArray(data) ? data : (data.data || []);
    
    return payouts.map(normalizePayout);
  },

  // GET /api/admin/payouts/paid?hostId=1&fromDate=2025-01-01&toDate=2025-12-31 - Admin xem danh sách booking đã thanh toán
  // hostId (optional): Lọc theo host cụ thể
  // fromDate, toDate (optional): Lọc theo khoảng thời gian (YYYY-MM-DD)
  getAdminPaidPayouts: async (options?: {
    hostId?: number;
    fromDate?: string; // YYYY-MM-DD
    toDate?: string; // YYYY-MM-DD
  }): Promise<HostPayoutDTO[]> => {
    const params: any = {};
    if (options?.hostId !== undefined && options?.hostId !== null) {
      params.hostId = options.hostId;
    }
    if (options?.fromDate) {
      params.fromDate = options.fromDate;
    }
    if (options?.toDate) {
      params.toDate = options.toDate;
    }
    
    const response = await axiosClient.get<any>("/admin/payouts/paid", { params });
    const data = response.data;
    
    // Normalize response từ backend (PascalCase -> camelCase)
    const payouts = Array.isArray(data) ? data : (data.data || []);
    
    return payouts.map(normalizePayout);
  },

  // POST /api/admin/payouts/process-all - Admin xử lý tất cả booking đủ điều kiện
  processAllPayouts: async (): Promise<ProcessPayoutResponse> => {
    const response = await axiosClient.post<any>("/admin/payouts/process-all");
    return normalizeProcessResponse(response.data);
  },

  // POST /api/admin/payouts/{bookingId}/confirm - Admin xác nhận và xử lý payout cho một booking cụ thể
  confirmPayout: async (bookingId: number): Promise<ProcessPayoutResponse> => {
    const response = await axiosClient.post<any>(`/admin/payouts/${bookingId}/confirm`);
    return normalizeProcessResponse(response.data);
  },

  // POST /api/admin/payouts/{bookingId}/reject - Admin từ chối payout cho một booking cụ thể
  rejectPayout: async (
    bookingId: number,
    reason: string
  ): Promise<{ success: boolean; message: string }> => {
    const response = await axiosClient.post<{ success: boolean; message: string }>(
      `/admin/payouts/${bookingId}/reject`,
      { reason }
    );
    return response.data;
  },
};

export default payoutAPI;

