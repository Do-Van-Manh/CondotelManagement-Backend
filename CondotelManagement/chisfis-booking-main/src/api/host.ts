import axiosClient from "./axiosClient";

// Host Verification DTOs
export interface HostVerificationResponseDTO {
  success: boolean;
  message: string;
  data?: {
    idCardFrontUrl?: string;
    idCardBackUrl?: string;
    verificationStatus?: string; // "Pending", "Approved", "Rejected"
    verifiedAt?: string;
    verificationNote?: string;
    // Thông tin OCR từ ảnh
    ocrData?: {
      fullName?: string;
      idNumber?: string;
      dateOfBirth?: string;
      address?: string;
      nationality?: string;
      gender?: string;
      issueDate?: string;
      issuePlace?: string;
    };
  };
}

export interface ValidateIdCardResponseDTO {
  isValid: boolean;
  message: string;
  details?: {
    nameMatch: boolean;
    idNumberMatch: boolean;
    dateOfBirthMatch?: boolean;
    vietQRVerified?: boolean;
    vietQRMessage?: string;
    userFullName?: string;
    idCardFullName?: string;
    idCardNumber?: string;
    errors?: string[];
  };
}

export interface HostVerificationStatusDTO {
  idCardFrontUrl?: string;
  idCardBackUrl?: string;
  verificationStatus: string; // "Pending", "Approved", "Rejected"
  verifiedAt?: string;
  verificationNote?: string;
}

// API Calls
export const hostAPI = {
  // POST /api/Host/verify-with-id-card - Upload ảnh CCCD và OCR
  verifyWithIdCard: async (
    frontFile: File,
    backFile: File
  ): Promise<HostVerificationResponseDTO> => {
    const formData = new FormData();
    formData.append("IdCardFront", frontFile);
    formData.append("IdCardBack", backFile);

    const response = await axiosClient.post<any>("/Host/verify-with-id-card", formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    });

    const data = response.data;

    // Backend trả về { success, message, data }
    return {
      success: data.success !== undefined ? data.success : data.Success !== undefined ? data.Success : true,
      message: data.message || data.Message || "Upload thành công",
      data: data.data || data.Data ? {
        idCardFrontUrl: data.data?.IdCardFrontUrl || data.data?.idCardFrontUrl || data.Data?.IdCardFrontUrl,
        idCardBackUrl: data.data?.IdCardBackUrl || data.data?.idCardBackUrl || data.Data?.IdCardBackUrl,
        verificationStatus: data.data?.VerificationStatus || data.data?.verificationStatus || data.Data?.VerificationStatus,
        verifiedAt: data.data?.VerifiedAt || data.data?.verifiedAt || data.Data?.VerifiedAt,
        verificationNote: data.data?.VerificationNote || data.data?.verificationNote || data.Data?.VerificationNote,
        ocrData: data.data?.OcrData || data.data?.ocrData || data.Data?.OcrData ? {
          fullName: data.data?.OcrData?.FullName || data.data?.ocrData?.fullName,
          idNumber: data.data?.OcrData?.IdNumber || data.data?.ocrData?.idNumber,
          dateOfBirth: data.data?.OcrData?.DateOfBirth || data.data?.ocrData?.dateOfBirth,
          address: data.data?.OcrData?.Address || data.data?.ocrData?.address,
          nationality: data.data?.OcrData?.Nationality || data.data?.ocrData?.nationality,
          gender: data.data?.OcrData?.Gender || data.data?.ocrData?.gender,
          issueDate: data.data?.OcrData?.IssueDate || data.data?.ocrData?.issueDate,
          issuePlace: data.data?.OcrData?.IssuePlace || data.data?.ocrData?.issuePlace,
        } : undefined,
      } : undefined,
    };
  },

  // GET /api/Host/validate-id-card - Validate thông tin CCCD với user và VietQR
  validateIdCard: async (): Promise<ValidateIdCardResponseDTO> => {
    const response = await axiosClient.get<any>("/Host/validate-id-card");
    const data = response.data;

    return {
      isValid: data.isValid !== undefined ? data.isValid : data.IsValid !== undefined ? data.IsValid : false,
      message: data.message || data.Message || "",
      details: data.details || data.Details ? {
        nameMatch: data.details?.nameMatch !== undefined ? data.details.nameMatch : data.Details?.NameMatch !== undefined ? data.Details.NameMatch : false,
        idNumberMatch: data.details?.idNumberMatch !== undefined ? data.details.idNumberMatch : data.Details?.IdNumberMatch !== undefined ? data.Details.IdNumberMatch : false,
        dateOfBirthMatch: data.details?.dateOfBirthMatch !== undefined ? data.details.dateOfBirthMatch : data.Details?.DateOfBirthMatch,
        vietQRVerified: data.details?.vietQRVerified !== undefined ? data.details.vietQRVerified : data.Details?.VietQRVerified,
        vietQRMessage: data.details?.vietQRMessage || data.Details?.VietQRMessage,
        userFullName: data.details?.userFullName || data.Details?.UserFullName,
        idCardFullName: data.details?.idCardFullName || data.Details?.IdCardFullName,
        idCardNumber: data.details?.idCardNumber || data.Details?.IdCardNumber,
        errors: data.details?.errors || data.Details?.Errors,
      } : undefined,
    };
  },

  // GET /api/Host/verification-status - Lấy trạng thái verification hiện tại (nếu có endpoint này)
  getVerificationStatus: async (): Promise<HostVerificationStatusDTO | null> => {
    try {
      // Có thể gọi từ endpoint riêng hoặc từ user profile
      // Tạm thời sẽ lấy từ validateIdCard response hoặc cần endpoint riêng
      const response = await axiosClient.get<any>("/Host/verification-status");
      const data = response.data;

      if (data.success && data.data) {
        return {
          idCardFrontUrl: data.data.IdCardFrontUrl || data.data.idCardFrontUrl,
          idCardBackUrl: data.data.IdCardBackUrl || data.data.idCardBackUrl,
          verificationStatus: data.data.VerificationStatus || data.data.verificationStatus || "Pending",
          verifiedAt: data.data.VerifiedAt || data.data.verifiedAt,
          verificationNote: data.data.VerificationNote || data.data.verificationNote,
        };
      }
      return null;
    } catch (err: any) {
      // Nếu endpoint không tồn tại, trả về null
      if (err.response?.status === 404) {
        return null;
      }
      throw err;
    }
  },
};

export default hostAPI;




