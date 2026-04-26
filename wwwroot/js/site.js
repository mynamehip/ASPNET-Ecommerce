(function () {
	const root = document.documentElement;
	const themeButtons = document.querySelectorAll(".theme-toggle");

	function syncThemeIcons() {
		const isDark = root.classList.contains("dark");
		document.querySelectorAll(".theme-icon-sun").forEach((item) => item.classList.toggle("hidden", !isDark));
		document.querySelectorAll(".theme-icon-moon").forEach((item) => item.classList.toggle("hidden", isDark));
	}

	function toggleTheme() {
		const isDark = root.classList.toggle("dark");
		localStorage.setItem("theme", isDark ? "dark" : "light");
		syncThemeIcons();
	}

	themeButtons.forEach((button) => {
		button.addEventListener("click", toggleTheme);
	});

	syncThemeIcons();

	const notificationsEnabled = document.body.dataset.notificationsEnabled === "true";
	const notificationHost = document.getElementById("notification-host");
	const notificationsTitle = document.body.dataset.notificationsTitle || "Live updates";

	function showNotification(payload) {
		if (!notificationHost) {
			return;
		}

		const toast = document.createElement("div");
		toast.className = "notification-toast pointer-events-auto rounded-2xl border border-gray-200 bg-white px-4 py-3 shadow-lg transition duration-300 translate-x-0 opacity-100";
		toast.innerHTML = `
			<div class="text-[11px] uppercase tracking-[0.18em] text-brand-500 font-bold">${notificationsTitle}</div>
			<div class="mt-1 text-sm font-semibold text-gray-900">${payload.message || "Update received"}</div>
			${payload.status ? `<div class="mt-1 text-xs text-gray-500">Status: ${payload.status}</div>` : ""}
		`;

		notificationHost.prepend(toast);

		window.setTimeout(() => {
			toast.classList.add("opacity-0", "translate-x-4");
			window.setTimeout(() => toast.remove(), 300);
		}, 5000);
	}

	window.updateWishlistCount = function (count) {
		const nextCount = Number.isFinite(Number(count)) ? Math.max(0, Number(count)) : 0;
		const desktopBadge = document.getElementById("wishlist-badge-count");
		const mobileBadge = document.getElementById("wishlist-menu-count");

		if (desktopBadge) {
			desktopBadge.textContent = String(nextCount);
			desktopBadge.classList.toggle("hidden", nextCount === 0);
			desktopBadge.classList.toggle("flex", nextCount > 0);
		}

		if (mobileBadge) {
			mobileBadge.textContent = String(nextCount);
			mobileBadge.classList.toggle("hidden", nextCount === 0);
			mobileBadge.classList.toggle("inline-flex", nextCount > 0);
		}
	};

	if (notificationsEnabled && window.signalR) {
		const connection = new signalR.HubConnectionBuilder()
			.withUrl("/hubs/notifications")
			.withAutomaticReconnect()
			.build();

		connection.on("notification", showNotification);

		connection.start().catch(() => {
			// Connection failures are non-blocking for the storefront shell.
		});
	}
})();
