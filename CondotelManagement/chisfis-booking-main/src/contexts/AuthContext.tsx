import React, { createContext, useContext, useState, useEffect, ReactNode } from "react";
import { authAPI, UserProfile } from "api/auth";
import axiosClient from "api/axiosClient";
import { packageAPI, HostPackageDetailsDto } from "api/package";

// === 1. ĐỊNH NGHĨA CONTEXT TYPE ===
interface AuthContextType {
  user: UserProfile | null;
  token: string | null;
  hostPackage: HostPackageDetailsDto | null;
  isLoading: boolean; // Đang khởi tạo auth
  isAuthenticated: boolean;
  isAdmin: boolean;
  login: (token: string, user: UserProfile) => void;
  logout: () => void;
  reloadUser: () => Promise<void>;
  updateUser: (user: UserProfile) => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) throw new Error("useAuth must be used within an AuthProvider");
  return context;
};

// === 2. AUTH PROVIDER ===
export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [user, setUser] = useState<UserProfile | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [hostPackage, setHostPackage] = useState<HostPackageDetailsDto | null>(null);
  const [isLoading, setIsLoading] = useState(true); // Bắt đầu là true

  // === KHỞI TẠO AUTH KHI APP LOAD ===
  useEffect(() => {
    const initializeAuth = async () => {
      const storedToken = localStorage.getItem("token");
      const userStr = localStorage.getItem("user");

      // Khôi phục user từ localStorage (nếu có)
      if (userStr) {
        try {
          const parsedUser = JSON.parse(userStr);
          setUser(parsedUser);
        } catch {
          console.warn("Invalid user data in localStorage");
        }
      }

      if (!storedToken) {
        setIsLoading(false);
        return;
      }

      // Có token → thiết lập header
      setToken(storedToken);
      axiosClient.defaults.headers.common["Authorization"] = `Bearer ${storedToken}`;

      try {
        // Lấy thông tin user mới nhất từ server
        const userProfile = await authAPI.getMe();
        setUser(userProfile);
        localStorage.setItem("user", JSON.stringify(userProfile));

        // CHỈ gọi packageAPI.getMyPackage() KHI USER LÀ HOST
        if (userProfile.roleName === "Host") {
          try {
            const pkg = await packageAPI.getMyPackage();
            setHostPackage(pkg);
            console.log("Host package loaded:", pkg);
          } catch (pkgError: any) {
            // Nếu 400 hoặc 404 → người dùng chưa có package → bình thường
            if (pkgError.response?.status === 400 || pkgError.response?.status === 404) {
              console.log("Host chưa có package → bình thường");
              setHostPackage(null);
            } else {
              console.warn("Lỗi không mong muốn khi load package:", pkgError);
            }
          }
        } else {
          setHostPackage(null); // Tenant, Admin → không cần package
        }
      } catch (error: any) {
        console.error("Token không hợp lệ hoặc hết hạn → đăng xuất", error);
        handleLogout();
      } finally {
        setIsLoading(false);
      }
    };

    initializeAuth();
  }, []); // Chỉ chạy 1 lần khi app load

  // === LOGIN ===
  const login = (newToken: string, newUser: UserProfile) => {
    const cleanToken = newToken.startsWith("Bearer ") ? newToken.substring(7) : newToken;
    setToken(cleanToken);
    setUser(newUser);
    localStorage.setItem("token", cleanToken);
    localStorage.setItem("user", JSON.stringify(newUser));
    axiosClient.defaults.headers.common["Authorization"] = `Bearer ${cleanToken}`;

    // Chỉ load package nếu là Host
    if (newUser.roleName === "Host") {
      packageAPI.getMyPackage()
        .then(setHostPackage)
        .catch(err => {
          if (err.response?.status === 400 || err.response?.status === 404) {
            setHostPackage(null);
          } else {
            console.warn("Lỗi load package sau login:", err);
          }
        });
    } else {
      setHostPackage(null);
    }
  };

  // === LOGOUT ===
  const handleLogout = async () => {
    const currentToken = localStorage.getItem("token");

    if (currentToken) {
      try {
        await authAPI.logout();
        console.log("Logout API success");
      } catch (error) {
        console.warn("Logout API failed, proceeding with local logout:", error);
      }
    }

    // Xóa toàn bộ dữ liệu
    setUser(null);
    setToken(null);
    setHostPackage(null);
    localStorage.removeItem("token");
    localStorage.removeItem("user");
    delete axiosClient.defaults.headers.common["Authorization"];

    // Chuyển hướng cứng (đảm bảo không cache)
    window.location.href = "/login";
  };

  // === RELOAD USER ===
  const reloadUser = async () => {
    if (!token) return;

    try {
      const userProfile = await authAPI.getMe();
      setUser(userProfile);
      localStorage.setItem("user", JSON.stringify(userProfile));

      if (userProfile.roleName === "Host") {
        try {
          const pkg = await packageAPI.getMyPackage();
          setHostPackage(pkg);
        } catch (err: any) {
          if (err.response?.status === 400 || err.response?.status === 404) {
            setHostPackage(null);
          } else {
            console.warn("Lỗi reload package:", err);
          }
        }
      } else {
        setHostPackage(null);
      }
    } catch (error) {
      console.error("Reload user thất bại → logout", error);
      handleLogout();
    }
  };

  // === UPDATE USER (Sau khi edit profile, upload ảnh, v.v.) ===
  const updateUser = (updatedUser: UserProfile) => {
    setUser(updatedUser);
    localStorage.setItem("user", JSON.stringify(updatedUser));
  };

  // === GIÁ TRỊ CUNG CẤP ===
  const value: AuthContextType = {
    user,
    token,
    hostPackage,
    isLoading,
    isAuthenticated: !!user && !!token,
    isAdmin: user?.roleName === "Admin",
    login,
    logout: handleLogout,
    reloadUser,
    updateUser,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};