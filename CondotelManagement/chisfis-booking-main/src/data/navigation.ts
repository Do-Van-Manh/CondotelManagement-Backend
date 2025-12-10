import { NavItemType } from "shared/Navigation/NavigationItem";
import ncNanoId from "utils/ncNanoId";
import type { Translations } from "i18n/LanguageContext";

// Function to get navigation items with translations
export const getNavigationItems = (t: Translations): NavItemType[] => {
  return [
    {
      id: ncNanoId(),
      href: "/",
      name: t.header.home,
    },
    {
      id: ncNanoId(),
      href: "/listing-stay",
      name: t.header.listing,
      type: "dropdown",
      children: [
        {
          id: ncNanoId(),
          href: "/listing-stay",
          name: t.header.allCondotels,
        },
        {
          id: ncNanoId(),
          href: "/listing-stay-map",
          name: t.header.map,
        },
      ],
    },
    {
      id: ncNanoId(),
      href: "/blog",
      name: t.header.blog,
    },
    {
      id: ncNanoId(),
      href: "/about",
      name: t.header.about,
    },
    {
      id: ncNanoId(),
      href: "/contact",
      name: t.header.contact,
    },
  ];
};

// Keep for backward compatibility (will use default Vietnamese)
export const NAVIGATION_DEMO: NavItemType[] = [
  {
    id: ncNanoId(),
    href: "/",
    name: "Trang chủ",
  },
  {
    id: ncNanoId(),
    href: "/listing-stay",
    name: "Danh sách Condotel",
    type: "dropdown",
    children: [
      {
        id: ncNanoId(),
        href: "/listing-stay",
        name: "Tất cả Condotel",
      },
      {
        id: ncNanoId(),
        href: "/listing-stay-map",
        name: "Bản đồ",
      },
    ],
  },
  {
    id: ncNanoId(),
    href: "/blog",
    name: "Blog",
  },
  {
    id: ncNanoId(),
    href: "/about",
    name: "Về chúng tôi",
  },
  {
    id: ncNanoId(),
    href: "/contact",
    name: "Liên hệ",
  },
];
