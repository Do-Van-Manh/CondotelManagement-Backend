import React, { createContext, useContext, useState, useEffect, ReactNode } from "react";
import moment from "moment";
import "moment/locale/vi";
import { vi } from "./translations/vi";
import { en } from "./translations/en";

export type Language = "vi" | "en";

export type Translations = typeof vi;

interface LanguageContextType {
  language: Language;
  setLanguage: (lang: Language) => void;
  t: Translations;
}

const LanguageContext = createContext<LanguageContextType | undefined>(undefined);

const translations: Record<Language, Translations> = {
  vi,
  en,
};

interface LanguageProviderProps {
  children: ReactNode;
}

export const LanguageProvider: React.FC<LanguageProviderProps> = ({ children }) => {
  const [language, setLanguageState] = useState<Language>(() => {
    // Lấy ngôn ngữ từ localStorage hoặc mặc định là 'vi'
    const savedLanguage = localStorage.getItem("language") as Language;
    return savedLanguage && (savedLanguage === "vi" || savedLanguage === "en") ? savedLanguage : "vi";
  });

  const setLanguage = (lang: Language) => {
    setLanguageState(lang);
    localStorage.setItem("language", lang);
    // Cập nhật moment locale
    if (lang === "vi") {
      moment.locale("vi");
    } else {
      moment.locale("en");
    }
  };

  useEffect(() => {
    // Set moment locale khi component mount
    if (language === "vi") {
      moment.locale("vi");
    } else {
      moment.locale("en");
    }
  }, [language]);

  const value: LanguageContextType = {
    language,
    setLanguage,
    t: translations[language],
  };

  return <LanguageContext.Provider value={value}>{children}</LanguageContext.Provider>;
};

export const useLanguage = (): LanguageContextType => {
  const context = useContext(LanguageContext);
  if (context === undefined) {
    throw new Error("useLanguage must be used within a LanguageProvider");
  }
  return context;
};

// Hook để sử dụng translations (alias cho useLanguage)
export const useTranslation = () => {
  const { t, language, setLanguage } = useLanguage();
  return { t, language, setLanguage };
};

