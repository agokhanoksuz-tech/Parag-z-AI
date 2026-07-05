import { useState } from "react";

export default function StoreLine({ item }) {
  const [iconFailed, setIconFailed] = useState(false);

  return (
    <p className="card-store">
      {item.storeIconUrl && !iconFailed && (
        <img
          className="card-store-icon"
          src={item.storeIconUrl}
          alt=""
          loading="lazy"
          onError={() => setIconFailed(true)}
        />
      )}
      {item.store}
    </p>
  );
}
