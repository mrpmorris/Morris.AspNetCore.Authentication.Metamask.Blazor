if (typeof window.ethereum !== "undefined") {

	const selectAccountButton = document.getElementById("selectAccountButton");
	if (selectAccountButton !== null) {
		selectAccountButton.style.display = "block";
		selectAccountButton.addEventListener(
			"click",
			async () => {
				const accounts = await window.ethereum.request({ method: 'eth_requestAccounts' });
				if (accounts.length == 0)
					return;

				const currentUrl = new URL(window.location.href);
				currentUrl.searchParams.set('accounts', accounts.join(','));
				window.location.href = currentUrl.toString();
			});
	}
}
