import React from "react";

export interface InputProps
  extends React.InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  as?: "input" | "textarea" | "select";
  options?: { label: string; value: string }[];
  // THÊM 3 DÒNG SAU:
  rounded?: string;
  fontClass?: string;
  sizeClass?: string;
  // Support textarea-specific props
  rows?: number;
  cols?: number;
}

const Input = React.forwardRef<HTMLInputElement, InputProps>(
  (
    {
      label,
      as = "input",
      options = [],
      className = "",
      type = "text",
      // THÊM 3 DÒNG SAU ĐỂ LẤY PROPS VÀ ĐẶT GIÁ TRỊ MẶC ĐỊNH:
      rounded = "rounded-2xl",
      fontClass = "text-sm",
      sizeClass = "px-4 py-3",
      rows,
      cols,
      ...rest
    },
    ref
  ) => {
    // TẠO BIẾN CLASSNAME CHUNG ĐỂ SỬ DỤNG LẠI
    const baseClassName = `block w-full border border-neutral-300 dark:border-neutral-700 bg-white dark:bg-neutral-900 focus:ring focus:ring-primary-200 focus:border-primary-400 ${rounded} ${fontClass} ${sizeClass} ${className}`;

    return (
      <div className="w-full space-y-1">
        {label && (
          <label className="text-neutral-800 dark:text-neutral-200 font-medium">
            {label}
          </label>
        )}

        {as === "textarea" ? (
          <textarea
            // SỬA DÒNG NÀY:
            className={baseClassName}
            rows={rows}
            cols={cols}
            {...(rest as React.TextareaHTMLAttributes<HTMLTextAreaElement>)}
          />
        ) : as === "select" ? (
          <select
            // SỬA DÒNG NÀY:
            className={baseClassName}
            {...(rest as React.SelectHTMLAttributes<HTMLSelectElement>)}
          >
            <option value="">Select...</option>
            {options.map((opt) => (
              <option key={opt.value} value={opt.value}>
                {opt.label}
              </option>
            ))}
          </select>
        ) : (
          <input
            ref={ref}
            type={type}
            // SỬA DÒNG NÀY:
            className={baseClassName}
            {...rest}
          />
        )}
      </div>
    );
  }
);

export default Input;