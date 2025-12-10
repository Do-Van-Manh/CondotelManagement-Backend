import axios from "axios";

// Fallback URL nếu .env không có
const baseURL = process.env.REACT_APP_API_URL || "http://localhost:7216/api";

console.log("🔧 API Base URL:", baseURL);
if (!process.env.REACT_APP_API_URL) {
  console.warn("⚠️ REACT_APP_API_URL không được set, đang dùng default:", baseURL);
  console.warn("💡 Tạo file .env với REACT_APP_API_URL=http://localhost:7216/api");
}

const axiosClient = axios.create({
  baseURL: baseURL,
  headers: {
    "Content-Type": "application/json",
  },
  timeout: 10000,
});

// Request interceptor - Add token to headers
axiosClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("token");
    if (token) {
      config.headers = config.headers || {};
      (config.headers as any).Authorization = `Bearer ${token}`;
      // Đảm bảo token luôn có Bearer prefix
      // Strip "Bearer " nếu token đã có prefix này (để tránh duplicate)
      const cleanToken = token.trim().startsWith("Bearer ")
        ? token.trim().substring(7).trim()
        : token.trim();

      // Luôn thêm Bearer prefix khi gửi request
      (config.headers as any).Authorization = `Bearer ${cleanToken}`;

      // Log để debug (chỉ log cho admin và auth endpoints)
      if (config.url?.includes("admin") || config.url?.includes("Auth") || config.url?.includes("Upload")) {
        console.log("🔑 Authorization Header set:", `Bearer ${cleanToken.substring(0, 30)}...`);
        console.log("🔑 Full Authorization:", (config.headers as any).Authorization);
      }
    } else {
      // Log nếu không có token cho auth/admin endpoints
      if (config.url?.includes("admin") || config.url?.includes("Auth")) {
        console.warn("⚠️ No token found for authenticated request:", config.url);
        const storedToken = localStorage.getItem("token");
        console.warn("⚠️ Token in localStorage:", storedToken ? `${storedToken.substring(0, 30)}...` : "null");
      }
    }

    // Don't set Content-Type for FormData, let browser set it with boundary
    if (config.data instanceof FormData && config.headers) {
      delete config.headers["Content-Type"];
    }

    // Log request for debugging
    console.log("📤 Request:", config.method?.toUpperCase(), config.url);

    if (config.data instanceof FormData) {
      console.log("📤 Uploading file:", (config.data as FormData).get("file"));
    }

    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor - Handle errors
axiosClient.interceptors.response.use(
  (response) => {
    console.log("✅ Response:", response.status, response.config.url);
    return response;
  },
  (error) => {
    console.error("❌ API Error:", error.response?.status, error.config?.url, error.response?.data);
    // Enhanced error logging
    if (error.response) {
      // Server responded with error status
      const status = error.response.status;
      const url = error.config?.url;
      const data = error.response.data; // Capture data here for reuse

      console.error("❌ API Error Response:", {
        status: status,
        statusText: error.response.statusText,
        url: url,
        data: data,
        errors: data?.errors, // Validation errors từ backend
      });

      // 🚨 BƯỚC DEBUG QUAN TRỌNG NHẤT: In chi tiết lỗi Validation (400 Bad Request)
      if (status === 400) {
        if (data?.errors) {
          console.error("  👉 CHI TIẾT LỖI VALIDATION (400):", data.errors);
          // Format validation errors for easier reading
          if (typeof data.errors === 'object') {
            const errorMessages = Object.entries(data.errors)
              .map(([key, value]: [string, any]) => {
                if (Array.isArray(value)) {
                  return `${key}: ${value.join(', ')}`;
                }
                return `${key}: ${value}`;
              })
              .join('\n');
            console.error("  📋 Validation Errors:\n", errorMessages);
          }
        }
        if (data?.message) {
          console.error("  📋 Error Message:", data.message);
        }
        if (data?.title) {
          console.error("  📋 Error Title:", data.title);
        }
      }

      // Handle 401 Unauthorized - token expired or invalid
      if (status === 401) {
        console.error("🔒 Unauthorized (401) - Token may be expired or invalid");
        console.error("🔒 Request URL:", url);
        console.error("🔒 Current token:", localStorage.getItem("token") ? "exists" : "missing");

        // Only logout if not already on login page to avoid redirect loops
        if (!window.location.pathname.includes("/login") && !window.location.pathname.includes("/register")) {
          console.warn("⚠️ Redirecting to login due to 401 error");
          localStorage.removeItem("token");
          localStorage.removeItem("user");
          // Use setTimeout to avoid navigation during render
          setTimeout(() => {
            window.location.href = "/login";
          }, 100);
        }
      }
    } else if (error.request) {
      // Request made but no response received
      console.error("❌ API Network Error (No Response):", {
        url: error.config?.url,
        method: error.config?.method,
        message: error.message,
        code: error.code,
      });

      // Set a more helpful error message
      error.noResponse = true;
      error.networkError = true;
    } else {
      // Error setting up request
      console.error("❌ API Request Setup Error:", {
        message: error.message,
        url: error.config?.url,
      });
    }

    return Promise.reject(error);
  }
);

// ✅ Export mặc định (bắt buộc để file thành module)
export default axiosClient;

// ✅ Dòng này là CHÌA KHÓA — buộc TS nhận file là module dù chưa detect import/export
export { };
