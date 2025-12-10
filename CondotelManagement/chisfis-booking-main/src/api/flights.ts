import axiosClient from "./axiosClient";
import axios from "axios";

export interface FlightSearchParams {
  from: string;
  to: string;
  startDate?: string; // YYYY-MM-DD
  endDate?: string;   // YYYY-MM-DD
  guests?: number;
  flightClass?: string; // Economy/Business
  tripType?: "roundTrip" | "oneWay";
}

export interface FlightSearchResultItem {
  id: string;
  airlines: { logo: string; name: string };
  price: string; // formatted
}

const demoFlights = (params: FlightSearchParams): FlightSearchResultItem[] => {
  const key = `${params.from}__${params.to}`;
  // Sử dụng logo URL từ internet để demo hoạt động ngay
  const VNA = { logo: "https://www.gstatic.com/flights/airline_logos/70px/VN.png", name: "Vietnam Airlines" } as const;
  const VJ = { logo: "https://www.gstatic.com/flights/airline_logos/70px/VJ.png", name: "VietJet Air" } as const;
  const BB = { logo: "https://www.gstatic.com/flights/airline_logos/70px/QH.png", name: "Bamboo Airways" } as const;
  const table: Record<string, FlightSearchResultItem[]> = {
    "Hà Nội (HAN)__TP. Hồ Chí Minh (SGN)": [
      { id: "han-sgn-1", airlines: VNA, price: "1,290,000₫" },
      { id: "han-sgn-2", airlines: VJ, price: "990,000₫" },
      { id: "han-sgn-3", airlines: BB, price: "1,150,000₫" },
    ],
    "TP. Hồ Chí Minh (SGN)__Hà Nội (HAN)": [
      { id: "sgn-han-1", airlines: VNA, price: "1,300,000₫" },
      { id: "sgn-han-2", airlines: VJ, price: "1,050,000₫" },
    ],
    "TP. Hồ Chí Minh (SGN)__Đà Nẵng (DAD)": [
      { id: "sgn-dad-1", airlines: VJ, price: "850,000₫" },
      { id: "sgn-dad-2", airlines: VNA, price: "1,050,000₫" },
    ],
    "Hà Nội (HAN)__Đà Nẵng (DAD)": [
      { id: "han-dad-1", airlines: VNA, price: "980,000₫" },
      { id: "han-dad-2", airlines: BB, price: "920,000₫" },
    ],
    "Hà Nội (HAN)__Nha Trang (CXR)": [
      { id: "han-cxr-1", airlines: BB, price: "1,150,000₫" },
      { id: "han-cxr-2", airlines: VNA, price: "1,350,000₫" },
    ],
    "Nha Trang (CXR)__Hà Nội (HAN)": [
      { id: "cxr-han-1", airlines: BB, price: "1,200,000₫" },
    ],
    "TP. Hồ Chí Minh (SGN)__Nha Trang (CXR)": [
      { id: "sgn-cxr-1", airlines: VJ, price: "650,000đ" },
      { id: "sgn-cxr-2", airlines: VNA, price: "950,000đ" },
    ],
    "TP. Hồ Chí Minh (SGN)__Phú Quốc (PQC)": [
      { id: "sgn-pqc-1", airlines: VJ, price: "1,200,000đ" },
      { id: "sgn-pqc-2", airlines: VNA, price: "1,500,000đ" },
    ],
    "Hà Nội (HAN)__Huế (HUI)": [
      { id: "han-hui-1", airlines: VNA, price: "1,400,000đ" },
      { id: "han-hui-2", airlines: BB, price: "1,250,000đ" },
    ],
    "TP. Hồ Chí Minh (SGN)__Cần Thơ (VCA)": [
      { id: "sgn-vca-1", airlines: VNA, price: "850,000đ" },
      { id: "sgn-vca-2", airlines: VJ, price: "650,000đ" },
    ],
    "Hà Nội (HAN)__Vinh (VII)": [
      { id: "han-vii-1", airlines: VNA, price: "1,800,000đ" },
    ],
    "Đà Nẵng (DAD)__TP. Hồ Chí Minh (SGN)": [
      { id: "dad-sgn-1", airlines: VJ, price: "880,000đ" },
      { id: "dad-sgn-2", airlines: VNA, price: "1,180,000đ" },
    ],
    "Đà Nẵng (DAD)__Hà Nội (HAN)": [
      { id: "dad-han-1", airlines: VNA, price: "1,150,000đ" },
      { id: "dad-han-2", airlines: BB, price: "1,000,000đ" },
    ],
  };
  return table[key] || [];
};

const flightsAPI = {
  search: async (params: FlightSearchParams): Promise<FlightSearchResultItem[]> => {
    // Short-circuit: if missing from/to, avoid any network and return VN demo
    if (!params.from || !params.to) {
      const filled: FlightSearchParams = {
        ...params,
        from: params.from || "Hà Nội (HAN)",
        to: params.to || "TP. Hồ Chí Minh (SGN)",
      };
      const demo = demoFlights(filled);
      if (demo.length) return demo;
    }
    // 1) Try RapidAPI Skyscanner if key is available
    const RAPID_KEY = process.env.REACT_APP_RAPIDAPI_KEY;
    const RAPID_HOST = process.env.REACT_APP_RAPIDAPI_HOST || "skyscanner80.p.rapidapi.com";
    if (RAPID_KEY) {
      try {
        const iata = (label: string) => {
          const m = label.match(/\(([^)]+)\)/);
          return m ? m[1] : label;
        };
        const origin = iata(params.from);
        const destination = iata(params.to);
        const dateISO = params.startDate || new Date(Date.now() + 7 * 86400000).toISOString().slice(0, 10);
        const url = `https://${RAPID_HOST}/v1/flights/searchFlights`;
        const r = await axios.get<any>(url, {
          params: {
            origin,
            destination,
            date: dateISO,
            adults: params.guests || 1,
            cabinClass: params.flightClass || "Economy",
          },
          headers: {
            "X-RapidAPI-Key": RAPID_KEY,
            "X-RapidAPI-Host": RAPID_HOST,
          },
          timeout: 10000,
        });
        const raw: any = r.data;
        // Best-effort mapping; if structure unexpected, fall back below
        const items: FlightSearchResultItem[] = Array.isArray(raw?.data)
          ? raw.data.slice(0, 10).map((it: any, idx: number) => ({
              id: String(it.id || idx),
              airlines: {
                logo:
                  it?.legs?.[0]?.carriers?.marketing?.[0]?.logoUrl ||
                  it?.itinerary?.leg?.[0]?.carriers?.marketing?.[0]?.logoUrl ||
                  "https://www.gstatic.com/flights/airline_logos/70px/VN.png",
                name:
                  it?.legs?.[0]?.carriers?.marketing?.[0]?.name ||
                  it?.itinerary?.leg?.[0]?.carriers?.marketing?.[0]?.name ||
                  "Airline",
              },
              price:
                it?.price?.formatted ||
                it?.price?.amount?.toLocaleString?.("vi-VN", { style: "currency", currency: "VND" }) ||
                "1,000,000đ",
            }))
          : [];
        if (items.length) return items;
      } catch (e) {
        console.warn("⚠️ RapidAPI Skyscanner failed, falling back to demo.", e);
      }
    }
    // 2) Optional custom backend (if available)
    try {
      const res = await axiosClient.get<FlightSearchResultItem[]>("/flights/search", { params });
      if (Array.isArray(res.data) && res.data.length) return res.data;
    } catch {}
    // 3) Demo fallback
    const demo = demoFlights(params);
    return demo.length
      ? demo
      : [
          { id: "demo-1", airlines: { logo: "https://www.gstatic.com/flights/airline_logos/70px/VN.png", name: "Vietnam Airlines" }, price: "1,000,000đ" },
        ];
  },
};

export default flightsAPI;
