import axiosClient from "./axiosClient";

// BookingDTO từ backend - khớp với C# DTO
export interface BookingDTO {
  bookingId: number;
  condotelId: number;
  customerId: number;
  startDate: string; // DateOnly format: YYYY-MM-DD
  endDate: string; // DateOnly format: YYYY-MM-DD
  totalPrice?: number; // decimal? in C#
  status: string; // "Pending", "Confirmed", "Cancelled", "Completed"
  promotionId?: number; // int? in C#
  isUsingRewardPoints: boolean;
  createdAt: string; // DateTime in C#
  canRefund?: boolean; // Field từ backend để check xem booking có thể hoàn tiền không
  refundStatus?: string | null; // "Pending", "Refunded", "Completed", hoặc null (chưa có refund request)
  
  // Thông tin condotel (nếu backend trả về khi join)
  condotelName?: string;
  condotelImageUrl?: string;
  condotelPricePerNight?: number;
  
  // Thông tin customer (nếu backend trả về khi join - cho host)
  customerName?: string;
  customerEmail?: string;
}

export interface ServicePackageBookingItem {
  serviceId: number;
  quantity: number;
}

export interface CreateBookingDTO {
  condotelId: number;
  startDate: string; // YYYY-MM-DD (DateOnly)
  endDate: string; // YYYY-MM-DD (DateOnly)
  promotionId?: number;
  voucherCode?: string; // Mã voucher để validate và áp dụng
  servicePackages?: ServicePackageBookingItem[]; // Danh sách service packages với quantity
  isUsingRewardPoints?: boolean;
  status?: string; // "Pending", "Confirmed", "Cancelled", "Completed" - defaults to "Pending"
  condotelName?: string; // Required by backend validation
}

export interface UpdateBookingDTO {
  bookingId: number;
  startDate?: string;
  endDate?: string;
  promotionId?: number;
  isUsingRewardPoints?: boolean;
  status?: string; // "Pending", "Confirmed", "Cancelled", "Completed"
}

export interface CheckAvailabilityResponse {
  condotelId: number;
  startDate: string; // DateOnly
  endDate: string; // DateOnly
  available: boolean;
}

// API Calls
export const bookingAPI = {
  // GET /api/booking/my - Lấy tất cả bookings của tenant hiện tại
  getMyBookings: async (): Promise<BookingDTO[]> => {
    const response = await axiosClient.get<any[]>("/booking/my");
    // Normalize response từ backend (PascalCase -> camelCase)
    return response.data.map((item: any) => ({
      bookingId: item.BookingId || item.bookingId,
      condotelId: item.CondotelId || item.condotelId,
      customerId: item.CustomerId || item.customerId,
      startDate: item.StartDate || item.startDate,
      endDate: item.EndDate || item.endDate,
      totalPrice: item.TotalPrice !== undefined ? item.TotalPrice : item.totalPrice,
      status: item.Status || item.status,
      promotionId: item.PromotionId !== undefined ? item.PromotionId : item.promotionId,
      isUsingRewardPoints: item.IsUsingRewardPoints !== undefined ? item.IsUsingRewardPoints : item.isUsingRewardPoints,
      createdAt: item.CreatedAt || item.createdAt,
      canRefund: item.CanRefund !== undefined ? item.CanRefund : item.canRefund,
      refundStatus: item.RefundStatus !== undefined ? (item.RefundStatus || null) : (item.refundStatus !== undefined ? (item.refundStatus || null) : null),
      // Thông tin condotel nếu có
      condotelName: item.CondotelName || item.condotelName,
      condotelImageUrl: item.CondotelImageUrl || item.condotelImageUrl,
      condotelPricePerNight: item.CondotelPricePerNight !== undefined ? item.CondotelPricePerNight : item.condotelPricePerNight,
    }));
  },

  // GET /api/booking/{id} - Lấy booking theo ID
  getBookingById: async (id: number): Promise<BookingDTO> => {
    const response = await axiosClient.get<any>(`/booking/${id}`);
    const data = response.data;
    // Normalize response từ backend (PascalCase -> camelCase)
    return {
      bookingId: data.BookingId || data.bookingId,
      condotelId: data.CondotelId || data.condotelId,
      customerId: data.CustomerId || data.customerId,
      startDate: data.StartDate || data.startDate,
      endDate: data.EndDate || data.endDate,
      totalPrice: data.TotalPrice !== undefined ? data.TotalPrice : data.totalPrice,
      status: data.Status || data.status,
      promotionId: data.PromotionId !== undefined ? data.PromotionId : data.promotionId,
      isUsingRewardPoints: data.IsUsingRewardPoints !== undefined ? data.IsUsingRewardPoints : data.isUsingRewardPoints,
      createdAt: data.CreatedAt || data.createdAt,
      canRefund: data.CanRefund !== undefined ? data.CanRefund : data.canRefund,
      refundStatus: data.RefundStatus !== undefined ? (data.RefundStatus || null) : (data.refundStatus !== undefined ? (data.refundStatus || null) : null),
      // Thông tin condotel nếu có
      condotelName: data.CondotelName || data.condotelName,
      condotelImageUrl: data.CondotelImageUrl || data.condotelImageUrl,
      condotelPricePerNight: data.CondotelPricePerNight !== undefined ? data.CondotelPricePerNight : data.condotelPricePerNight,
    };
  },

  // GET /api/booking/check-availability - Kiểm tra tính khả dụng
  checkAvailability: async (
    condotelId: number,
    startDate: string,
    endDate: string
  ): Promise<CheckAvailabilityResponse> => {
    const response = await axiosClient.get<CheckAvailabilityResponse>(
      "/booking/check-availability",
      {
        params: {
          condotelId,
          checkIn: startDate, // Backend có thể dùng checkIn/checkOut trong query params
          checkOut: endDate,
        },
      }
    );
    return response.data;
  },

  // POST /api/booking - Tạo booking mới
  createBooking: async (booking: CreateBookingDTO): Promise<BookingDTO> => {
    // Map camelCase sang PascalCase để khớp với backend C# DTO
    const requestData: any = {
      CondotelId: booking.condotelId,
      StartDate: booking.startDate,
      EndDate: booking.endDate,
      Status: booking.status || "Pending", // Default to "Pending" for new bookings
    };

    // Backend requires CondotelName for validation
    if (booking.condotelName) {
      requestData.CondotelName = booking.condotelName;
    }

    if (booking.promotionId !== undefined) {
      requestData.PromotionId = booking.promotionId;
    }
    if (booking.voucherCode) {
      requestData.VoucherCode = booking.voucherCode;
    }
    if (booking.servicePackages && booking.servicePackages.length > 0) {
      requestData.ServicePackages = booking.servicePackages.map(sp => ({
        ServiceId: sp.serviceId,
        Quantity: sp.quantity,
      }));
    }
    if (booking.isUsingRewardPoints !== undefined) {
      requestData.IsUsingRewardPoints = booking.isUsingRewardPoints;
    }

    console.log("📤 Creating booking with data:", JSON.stringify(requestData, null, 2));
    console.log("🎫 Voucher code being sent:", booking.voucherCode || "None");
    console.log("📦 Service packages being sent:", booking.servicePackages?.length || 0);

    const response = await axiosClient.post<any>("/booking", requestData);
    console.log("✅ Booking created successfully:", response.data);

    // Backend returns result with nested Data property (ServiceResult pattern)
    // Response structure: { success: true, data: BookingDTO, message: ... }
    // Or direct BookingDTO if CreatedAtAction returns it directly
    const responseData: any = response.data;
    
    // Extract booking data - could be in responseData.data or responseData directly
    const data: any = responseData.data || responseData;
    
    console.log("📦 Extracted booking data:", data);
    
    // Normalize response từ backend (PascalCase -> camelCase)
    const bookingId = data.BookingId || data.bookingId;
    if (!bookingId) {
      console.error("❌ BookingId not found in response:", responseData);
      throw new Error("Booking created but BookingId not found in response");
    }
    
    return {
      bookingId: bookingId,
      condotelId: data.CondotelId || data.condotelId,
      customerId: data.CustomerId || data.customerId,
      startDate: data.StartDate || data.startDate,
      endDate: data.EndDate || data.endDate,
      totalPrice: data.TotalPrice !== undefined ? data.TotalPrice : data.totalPrice,
      status: data.Status || data.status,
      promotionId: data.PromotionId !== undefined ? data.PromotionId : data.promotionId,
      isUsingRewardPoints: data.IsUsingRewardPoints !== undefined ? data.IsUsingRewardPoints : data.isUsingRewardPoints,
      createdAt: data.CreatedAt || data.createdAt,
      canRefund: data.CanRefund !== undefined ? data.CanRefund : data.canRefund,
      refundStatus: data.RefundStatus !== undefined ? (data.RefundStatus || null) : (data.refundStatus !== undefined ? (data.refundStatus || null) : null),
    };
  },

  // PUT /api/booking/{id} - Cập nhật booking
  updateBooking: async (
    id: number,
    booking: UpdateBookingDTO
  ): Promise<BookingDTO> => {
    // Map camelCase sang PascalCase
    const requestData: any = {
      BookingId: id,
    };

    if (booking.startDate) {
      requestData.StartDate = booking.startDate;
    }
    if (booking.endDate) {
      requestData.EndDate = booking.endDate;
    }
    if (booking.promotionId !== undefined) {
      requestData.PromotionId = booking.promotionId;
    }
    if (booking.isUsingRewardPoints !== undefined) {
      requestData.IsUsingRewardPoints = booking.isUsingRewardPoints;
    }

    const response = await axiosClient.put<any>(`/booking/${id}`, requestData);
    const data: any = response.data;
    // Normalize response từ backend (PascalCase -> camelCase)
    return {
      bookingId: data.BookingId || data.bookingId,
      condotelId: data.CondotelId || data.condotelId,
      customerId: data.CustomerId || data.customerId,
      startDate: data.StartDate || data.startDate,
      endDate: data.EndDate || data.endDate,
      totalPrice: data.TotalPrice !== undefined ? data.TotalPrice : data.totalPrice,
      status: data.Status || data.status,
      promotionId: data.PromotionId !== undefined ? data.PromotionId : data.promotionId,
      isUsingRewardPoints: data.IsUsingRewardPoints !== undefined ? data.IsUsingRewardPoints : data.isUsingRewardPoints,
      createdAt: data.CreatedAt || data.createdAt,
      canRefund: data.CanRefund !== undefined ? data.CanRefund : data.canRefund,
      refundStatus: data.RefundStatus !== undefined ? (data.RefundStatus || null) : (data.refundStatus !== undefined ? (data.refundStatus || null) : null),
    };
  },

  // DELETE /api/booking/{id} - Hủy booking
  cancelBooking: async (id: number): Promise<void> => {
    await axiosClient.delete(`/booking/${id}`);
  },

  // POST /api/booking/{id}/refund - Yêu cầu hoàn tiền cho booking đã hủy
  refundBooking: async (
    id: number, 
    bankInfo?: { bankName: string; accountNumber: string; accountHolder: string }
  ): Promise<{ success: boolean; message?: string; data?: any; bankInfo?: any }> => {
    try {
      // Nếu có thông tin ngân hàng, gửi kèm trong body
      // Backend expect: BankCode (không phải BankName), AccountNumber, AccountHolder
      const payload = bankInfo ? {
        BankCode: bankInfo.bankName, // bankName từ frontend là mã ngân hàng (VCB, MB, etc.)
        AccountNumber: bankInfo.accountNumber,
        AccountHolder: bankInfo.accountHolder,
      } : {};
      
      // Log payload (ẩn thông tin nhạy cảm)
      console.log("📤 Sending refund request:", {
        bookingId: id,
        payload: {
          BankCode: payload.BankCode,
          AccountNumber: payload.AccountNumber ? payload.AccountNumber.substring(0, 3) + "***" : undefined,
          AccountHolder: payload.AccountHolder ? payload.AccountHolder.substring(0, 3) + "***" : undefined,
        },
        hasBankInfo: !!bankInfo,
        bankInfoProvided: {
          bankName: bankInfo?.bankName,
          hasAccountNumber: !!bankInfo?.accountNumber,
          hasAccountHolder: !!bankInfo?.accountHolder,
        }
      });
      
      // Log full payload để debug (chỉ trong development)
      if (process.env.NODE_ENV === 'development') {
        console.log("📤 Full payload (dev only):", JSON.stringify(payload, null, 2));
      }
      
      const response = await axiosClient.post<any>(`/booking/${id}/refund`, payload);
      const data = response.data;
      
      console.log("📥 Refund API response:", data);
      console.log("📥 Full response data:", JSON.stringify(data, null, 2));
      
      // Log để verify bank info có được gửi và backend có nhận được không
      if (bankInfo) {
        const responseData = data.Data || data.data || {};
        // Backend trả về BankInfo object với BankCode, AccountNumber, AccountHolder
        const receivedBankInfo = responseData.BankInfo || responseData.bankInfo || {};
        
        console.log("🔍 Verifying bank info in request:", {
          sent: {
            BankCode: payload.BankCode,
            AccountNumber: payload.AccountNumber ? payload.AccountNumber.substring(0, 3) + "***" : undefined,
            AccountHolder: payload.AccountHolder ? payload.AccountHolder.substring(0, 3) + "***" : undefined,
          },
          received: {
            BankCode: receivedBankInfo.BankCode || receivedBankInfo.bankCode,
            AccountNumber: receivedBankInfo.AccountNumber || receivedBankInfo.accountNumber 
              ? (receivedBankInfo.AccountNumber || receivedBankInfo.accountNumber).substring(0, 3) + "***" 
              : undefined,
            AccountHolder: receivedBankInfo.AccountHolder || receivedBankInfo.accountHolder
              ? (receivedBankInfo.AccountHolder || receivedBankInfo.accountHolder).substring(0, 3) + "***"
              : undefined,
          },
          responseData: responseData,
          responseSuccess: data.Success !== undefined ? data.Success : data.success,
          responseMessage: data.Message || data.message,
        });
        
        // Verify bank info được lưu
        const receivedBankCode = receivedBankInfo.BankCode || receivedBankInfo.bankCode;
        if (receivedBankCode) {
          console.log("✅ Bank info đã được lưu vào database và trả về trong response:", {
            BankCode: receivedBankCode,
            hasAccountNumber: !!(receivedBankInfo.AccountNumber || receivedBankInfo.accountNumber),
            hasAccountHolder: !!(receivedBankInfo.AccountHolder || receivedBankInfo.accountHolder),
          });
        } else {
          console.warn("⚠️ Backend response không chứa bank info. Có thể backend chưa lưu vào database.");
        }
      }
      
      // Backend trả về ServiceResult: { Success: bool, Message: string, Data?: any }
      // Data có thể chứa BankInfo object với BankCode, AccountNumber, AccountHolder
      const responseData = data.Data || data.data || {};
      return {
        success: data.Success !== undefined ? data.Success : data.success !== undefined ? data.success : true,
        message: data.Message || data.message,
        data: responseData,
        // Thêm bankInfo để dễ truy cập
        bankInfo: responseData.BankInfo || responseData.bankInfo || null,
      };
    } catch (error: any) {
      console.error("❌ Error in refundBooking API:", error);
      console.error("❌ Error response:", error.response?.data);
      
      // Nếu có response từ server, trả về message từ server
      if (error.response?.data) {
        const serverData = error.response.data;
        return {
          success: false,
          message: serverData.Message || serverData.message || serverData.title || "Không thể gửi yêu cầu hoàn tiền",
          data: serverData.Data || serverData.data,
        };
      }
      
      // Nếu không có response, throw error để handle ở component
      throw error;
    }
  },

  // ========== HOST BOOKING APIs ==========
  // GET /api/host/booking - Lấy tất cả bookings của host hiện tại
  getHostBookings: async (): Promise<BookingDTO[]> => {
    const response = await axiosClient.get<any>("/host/booking");
    // Normalize response từ backend (PascalCase -> camelCase)
    // Handle both array and object with data property
    let data: any[] = [];
    if (Array.isArray(response.data)) {
      data = response.data;
    } else if (response.data && Array.isArray(response.data.data)) {
      data = response.data.data;
    } else if (response.data && typeof response.data === 'object') {
      // If response.data is a single object, wrap it in array
      data = [response.data];
    }
    
    if (!Array.isArray(data)) {
      console.warn("getHostBookings: response.data is not an array:", response.data);
      return [];
    }
    
    return data.map((item: any) => ({
      bookingId: item.BookingId || item.bookingId,
      condotelId: item.CondotelId || item.condotelId,
      customerId: item.CustomerId || item.customerId,
      startDate: item.StartDate || item.startDate,
      endDate: item.EndDate || item.endDate,
      totalPrice: item.TotalPrice !== undefined ? item.TotalPrice : item.totalPrice,
      status: item.Status || item.status,
      promotionId: item.PromotionId !== undefined ? item.PromotionId : item.promotionId,
      isUsingRewardPoints: item.IsUsingRewardPoints !== undefined ? item.IsUsingRewardPoints : item.isUsingRewardPoints,
      createdAt: item.CreatedAt || item.createdAt,
      canRefund: item.CanRefund !== undefined ? item.CanRefund : item.canRefund,
      refundStatus: item.RefundStatus !== undefined ? (item.RefundStatus || null) : (item.refundStatus !== undefined ? (item.refundStatus || null) : null),
      // Thông tin condotel và customer nếu có
      condotelName: item.CondotelName || item.condotelName,
      condotelImageUrl: item.CondotelImageUrl || item.condotelImageUrl,
      condotelPricePerNight: item.CondotelPricePerNight !== undefined ? item.CondotelPricePerNight : item.condotelPricePerNight,
      customerName: item.CustomerName || item.customerName,
      customerEmail: item.CustomerEmail || item.customerEmail,
    }));
  },

  // GET /api/host/booking/customer/{customerId} - Lấy bookings theo customer
  getHostBookingsByCustomer: async (customerId: number): Promise<BookingDTO[]> => {
    const response = await axiosClient.get<any>(`/host/booking/customer/${customerId}`);
    // Normalize response từ backend (PascalCase -> camelCase)
    // Handle both array and object with data property
    let data: any[] = [];
    if (Array.isArray(response.data)) {
      data = response.data;
    } else if (response.data && Array.isArray(response.data.data)) {
      data = response.data.data;
    } else if (response.data && typeof response.data === 'object') {
      // If response.data is a single object, wrap it in array
      data = [response.data];
    }
    
    if (!Array.isArray(data)) {
      console.warn("getHostBookingsByCustomer: response.data is not an array:", response.data);
      return [];
    }
    
    return data.map((item: any) => ({
      bookingId: item.BookingId || item.bookingId,
      condotelId: item.CondotelId || item.condotelId,
      customerId: item.CustomerId || item.customerId,
      startDate: item.StartDate || item.startDate,
      endDate: item.EndDate || item.endDate,
      totalPrice: item.TotalPrice !== undefined ? item.TotalPrice : item.totalPrice,
      status: item.Status || item.status,
      promotionId: item.PromotionId !== undefined ? item.PromotionId : item.promotionId,
      isUsingRewardPoints: item.IsUsingRewardPoints !== undefined ? item.IsUsingRewardPoints : item.isUsingRewardPoints,
      createdAt: item.CreatedAt || item.createdAt,
      canRefund: item.CanRefund !== undefined ? item.CanRefund : item.canRefund,
      condotelName: item.CondotelName || item.condotelName,
      condotelImageUrl: item.CondotelImageUrl || item.condotelImageUrl,
      condotelPricePerNight: item.CondotelPricePerNight !== undefined ? item.CondotelPricePerNight : item.condotelPricePerNight,
      customerName: item.CustomerName || item.customerName,
      customerEmail: item.CustomerEmail || item.customerEmail,
    }));
  },

  // PUT /api/host/booking/{id} - Host cập nhật booking status
  updateHostBookingStatus: async (id: number, status: string): Promise<BookingDTO> => {
    const requestData: any = {
      BookingId: id,
      Status: status,
    };

    const response = await axiosClient.put<any>(`/host/booking/${id}`, requestData);
    const data: any = response.data;
    // Normalize response từ backend (PascalCase -> camelCase)
    return {
      bookingId: data.BookingId || data.bookingId,
      condotelId: data.CondotelId || data.condotelId,
      customerId: data.CustomerId || data.customerId,
      startDate: data.StartDate || data.startDate,
      endDate: data.EndDate || data.endDate,
      totalPrice: data.TotalPrice !== undefined ? data.TotalPrice : data.totalPrice,
      status: data.Status || data.status,
      promotionId: data.PromotionId !== undefined ? data.PromotionId : data.promotionId,
      isUsingRewardPoints: data.IsUsingRewardPoints !== undefined ? data.IsUsingRewardPoints : data.isUsingRewardPoints,
      createdAt: data.CreatedAt || data.createdAt,
      canRefund: data.CanRefund !== undefined ? data.CanRefund : data.canRefund,
      refundStatus: data.RefundStatus !== undefined ? (data.RefundStatus || null) : (data.refundStatus !== undefined ? (data.refundStatus || null) : null),
      condotelName: data.CondotelName || data.condotelName,
      condotelImageUrl: data.CondotelImageUrl || data.condotelImageUrl,
      condotelPricePerNight: data.CondotelPricePerNight !== undefined ? data.CondotelPricePerNight : data.condotelPricePerNight,
      customerName: data.CustomerName || data.customerName,
      customerEmail: data.CustomerEmail || data.customerEmail,
    };
  },

  // GET /api/booking/{id}/can-refund - Check xem booking có thể hoàn tiền không (Option 2)
  checkCanRefund: async (id: number): Promise<{ canRefund: boolean; message?: string }> => {
    const response = await axiosClient.get<any>(`/booking/${id}/can-refund`);
    const data = response.data;
    // Normalize response từ backend (PascalCase -> camelCase)
    return {
      canRefund: data.CanRefund !== undefined ? data.CanRefund : data.canRefund,
      message: data.Message || data.message,
    };
  },
};

export default bookingAPI;

