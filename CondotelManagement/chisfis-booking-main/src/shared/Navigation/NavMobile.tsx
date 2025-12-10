import React from "react";
import ButtonClose from "shared/ButtonClose/ButtonClose";
import Logo from "shared/Logo/Logo";
import { Disclosure } from "@headlessui/react";
import { NavLink, useNavigate } from "react-router-dom";
import { NavItemType } from "./NavigationItem";
import { getNavigationItems } from "data/navigation";
import ButtonPrimary from "shared/Button/ButtonPrimary";
import SocialsList from "shared/SocialsList/SocialsList";
import { ChevronDownIcon } from "@heroicons/react/24/solid";
import { ArrowRightOnRectangleIcon, UserCircleIcon, DocumentTextIcon, HomeIcon } from "@heroicons/react/24/outline";
import SwitchDarkMode from "shared/SwitchDarkMode/SwitchDarkMode";
import LangDropdown from "components/Header/LangDropdown";
import { useAuth } from "contexts/AuthContext";
import Avatar from "shared/Avatar/Avatar";
import { useTranslation } from "i18n/LanguageContext";

export interface NavMobileProps {
  data?: NavItemType[];
  onClickClose?: () => void;
}

const NavMobile: React.FC<NavMobileProps> = ({
  data,
  onClickClose,
}) => {
  const { user, isAuthenticated, isAdmin, logout } = useAuth();
  const { t } = useTranslation();
  const navigate = useNavigate();
  
  // Use translations if data not provided
  const navigationData = data || getNavigationItems(t);

  const handleLogout = async () => {
    if (onClickClose) {
      onClickClose();
    }
    await logout();
  };

  const handleAccountClick = () => {
    if (onClickClose) {
      onClickClose();
    }
    if (isAdmin) {
      navigate("/admin?tab=profile");
    } else if (user?.roleName === "Host") {
      navigate("/host-dashboard");
    } else {
      navigate("/account");
    }
  };
  const _renderMenuChild = (item: NavItemType) => {
    return (
      <ul className="nav-mobile-sub-menu pl-6 pb-1 text-base">
        {item.children?.map((i, index) => (
          <Disclosure key={i.href + index} as="li">
            <NavLink
              end
              to={{
                pathname: i.href || undefined,
              }}
              className={({ isActive }) =>
                `flex px-4 text-neutral-900 dark:text-neutral-200 text-sm font-medium rounded-lg hover:bg-neutral-100 dark:hover:bg-neutral-800 mt-0.5 ${
                  isActive ? "text-secondary" : ""
                }`
              }
            >
              <span
                className={`py-2.5 pr-3 ${!i.children ? "block w-full" : ""}`}
              >
                {i.name}
              </span>
              {i.children && (
                <span
                  className="flex-1 flex"
                  onClick={(e) => e.preventDefault()}
                >
                  <Disclosure.Button
                    as="span"
                    className="py-2.5 flex justify-end flex-1"
                  >
                    <ChevronDownIcon
                      className="ml-2 h-4 w-4 text-neutral-500"
                      aria-hidden="true"
                    />
                  </Disclosure.Button>
                </span>
              )}
            </NavLink>
            {i.children && (
              <Disclosure.Panel>{_renderMenuChild(i)}</Disclosure.Panel>
            )}
          </Disclosure>
        ))}
      </ul>
    );
  };

  const _renderItem = (item: NavItemType, index: number) => {
    return (
      <Disclosure
        key={item.id}
        as="li"
        className="text-neutral-900 dark:text-white"
      >
        <NavLink
          end
          className={({ isActive }) =>
            `flex w-full px-4 font-medium uppercase tracking-wide text-sm hover:bg-neutral-100 dark:hover:bg-neutral-800 rounded-lg ${
              isActive ? "text-secondary" : ""
            }`
          }
          to={{
            pathname: item.href || undefined,
          }}
        >
          <span
            className={`py-2.5 pr-3 ${!item.children ? "block w-full" : ""}`}
          >
            {item.name}
          </span>
          {item.children && (
            <span className="flex-1 flex" onClick={(e) => e.preventDefault()}>
              <Disclosure.Button
                as="span"
                className="py-2.5 flex items-center justify-end flex-1 "
              >
                <ChevronDownIcon
                  className="ml-2 h-4 w-4 text-neutral-500"
                  aria-hidden="true"
                />
              </Disclosure.Button>
            </span>
          )}
        </NavLink>
        {item.children && (
          <Disclosure.Panel>{_renderMenuChild(item)}</Disclosure.Panel>
        )}
      </Disclosure>
    );
  };

  return (
    <div className="overflow-y-auto w-full h-screen py-2 transition transform shadow-lg ring-1 dark:ring-neutral-700 bg-white dark:bg-neutral-900 divide-y-2 divide-neutral-100 dark:divide-neutral-800">
      <div className="py-6 px-5">
        <Logo />
        <div className="flex flex-col mt-5 text-neutral-700 dark:text-neutral-300 text-sm">
          <span>
            Discover the most outstanding articles on all topics of life. Write
            your stories and share them
          </span>

          <div className="flex justify-between items-center mt-4">
            <SocialsList itemClass="w-9 h-9 flex items-center justify-center rounded-full bg-neutral-100 text-xl dark:bg-neutral-800 dark:text-neutral-300" />
            <span className="block">
              <SwitchDarkMode className="bg-neutral-100 dark:bg-neutral-800" />
            </span>
          </div>
        </div>
        <span className="absolute right-2 top-2 p-1">
          <ButtonClose onClick={onClickClose} />
        </span>
      </div>
      <ul className="flex flex-col py-6 px-2 space-y-1">
        {navigationData.map(_renderItem)}
      </ul>

      {/* User Account Section for Authenticated Users */}
      {isAuthenticated && user && (
        <div className="border-t border-neutral-200 dark:border-neutral-700 py-4 px-5">
          <div className="flex items-center space-x-3 mb-3">
            <Avatar
              sizeClass="w-10 h-10"
              imgUrl={user.imageUrl}
              userName={user.fullName}
            />
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium text-neutral-900 dark:text-neutral-100 truncate">
                {user.fullName || "User"}
              </p>
              <p className="text-xs text-neutral-500 dark:text-neutral-400">
                {user.roleName || "User"}
              </p>
            </div>
          </div>
          <div className="flex flex-col space-y-2">
            <button
              onClick={handleAccountClick}
              className="flex items-center w-full px-4 py-2 text-sm text-neutral-700 dark:text-neutral-300 hover:bg-neutral-100 dark:hover:bg-neutral-800 rounded-lg transition-colors"
            >
              <UserCircleIcon className="w-5 h-5 mr-3" />
              <span>{t.account.profile}</span>
            </button>
            {user?.roleName === "Host" && (
              <button
                onClick={() => {
                  if (onClickClose) onClickClose();
                  navigate("/host-dashboard");
                }}
                className="flex items-center w-full px-4 py-2 text-sm text-neutral-700 dark:text-neutral-300 hover:bg-neutral-100 dark:hover:bg-neutral-800 rounded-lg transition-colors"
              >
                <HomeIcon className="w-5 h-5 mr-3" />
                <span>{t.header.dashboard}</span>
              </button>
            )}
            {user?.roleName !== "Host" && user?.roleName !== "Admin" && (
              <button
                onClick={() => {
                  if (onClickClose) onClickClose();
                  navigate("/my-bookings");
                }}
                className="flex items-center w-full px-4 py-2 text-sm text-neutral-700 dark:text-neutral-300 hover:bg-neutral-100 dark:hover:bg-neutral-800 rounded-lg transition-colors"
              >
                <DocumentTextIcon className="w-5 h-5 mr-3" />
                <span>{t.header.myBookings}</span>
              </button>
            )}
            <button
              onClick={handleLogout}
              className="flex items-center w-full px-4 py-2 text-sm text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-lg transition-colors"
            >
              <ArrowRightOnRectangleIcon className="w-5 h-5 mr-3" />
              <span>{t.header.logout}</span>
            </button>
          </div>
        </div>
      )}

      <div className="flex items-center justify-between py-6 px-5">
        <a
          className="inline-block"
          href="https://themeforest.net/item/chisfis-online-booking-react-template/33515927"
          target="_blank"
          rel="noopener noreferrer"
        >
          <ButtonPrimary>Get Template</ButtonPrimary>
        </a>

        <LangDropdown panelClassName="z-10 w-screen max-w-[280px] px-4 mb-3 -right-3 bottom-full sm:px-0" />
      </div>
    </div>
  );
};

export default NavMobile;
