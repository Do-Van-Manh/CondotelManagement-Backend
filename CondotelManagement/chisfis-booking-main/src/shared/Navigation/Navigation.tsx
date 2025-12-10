import React from "react";
import NavigationItem from "./NavigationItem";
import { getNavigationItems } from "data/navigation";
import { useTranslation } from "i18n/LanguageContext";

function Navigation() {
  const { t } = useTranslation();
  const navigationItems = getNavigationItems(t);

  return (
    <ul className="nc-Navigation hidden lg:flex lg:flex-wrap lg:items-center lg:space-x-1 relative">
      {navigationItems.map((item) => (
        <NavigationItem key={item.id} menuItem={item} />
      ))}
    </ul>
  );
}

export default Navigation;
