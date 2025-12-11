import axiosClient from "./axiosClient";

export interface UploadImageResponse {
  message: string;
  imageUrl: string;
}

// API Calls for Upload
export const uploadAPI = {
  // POST /api/Upload/user-image
  // Upload ảnh cho user hiện tại (yêu cầu authentication)
  // Backend trả về: { message: string, imageUrl: string }
  uploadUserImage: async (file: File): Promise<UploadImageResponse> => {
    const formData = new FormData();
    formData.append("file", file);

    const response = await axiosClient.post<any>("/Upload/user-image", formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    });

    const data = response.data;
    
    // Normalize response (PascalCase -> camelCase)
    return {
      message: data.message || data.Message || "Upload thành công",
      imageUrl: data.imageUrl || data.ImageUrl || data.imageURL || "",
    };
  },

  // POST /api/Upload/image
  // Upload ảnh chung (không cần authentication)
  uploadImage: async (file: File): Promise<{ imageUrl: string }> => {
    const formData = new FormData();
    formData.append("file", file);

    const response = await axiosClient.post<any>(
      "/Upload/image",
      formData,
      {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      }
    );

    const data = response.data;
    
    // Normalize response (PascalCase -> camelCase)
    return {
      imageUrl: data.imageUrl || data.ImageUrl || data.imageURL || "",
    };
  },
};

export default uploadAPI;

