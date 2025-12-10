// src/components/ChatButtonFloating/ChatButtonFloating.tsx

import React from "react";
import { Link, useLocation } from "react-router-dom";

const ChatButtonFloating = () => {
    const location = useLocation();

    // Ẩn nút khi đang ở trang /chat
    if (location.pathname === "/chat") {
        return null;
    }

    return (
        <Link
            to="/chat"
            className="fixed bottom-6 right-6 z-50 flex items-center justify-center w-14 h-14 bg-primary-6000 hover:bg-primary-700 text-white rounded-full shadow-2xl transition-all duration-300 hover:scale-110"
        >
            {/* Dùng icon chat có sẵn trong template (từ HeroIcons hoặc Lucide) */}
            <svg
                xmlns="http://www.w3.org/2000/svg"
                className="w-8 h-8"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
            >
                <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z"
                />
            </svg>

            {/* Optional: badge tin nhắn mới */}
            {/* <span className="absolute -top-2 -right-2 bg-red-500 text-white text-xs font-bold rounded-full w-6 h-6 flex items-center justify-center">
        5
      </span> */}
        </Link>
    );
};

export default ChatButtonFloating;