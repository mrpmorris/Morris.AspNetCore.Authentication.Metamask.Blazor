const morrisMetamaskSelectWalletId = 'morris-metamask-select-wallet';
const morrisMetamaskSignPayloadId = 'morris-metamask-sign-payload';
const morrisMetamaskSignPayloadRedirectFormId = 'morris-metamask-sign-payload-redirect-form';

function fixUpMorrisMetamaskSelectWalletUI() {
	if (document.getElementById(morrisMetamaskSelectWalletId)) {
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
	}
}

function fixUpMorrisMetamaskSignPayloadUI() {
	if (document.getElementById(morrisMetamaskSignPayloadId)) {
		const hasEthereum = (typeof window.ethereum !== "undefined");
		const accountInput = document.getElementById("accountInput");
		const payloadInput = document.getElementById("payloadInput");
		const signatureInput = document.getElementById("signatureInput");
		const signInButton = document.getElementById("signInButton");
		const copyPayloadButton = document.getElementById("copyPayloadButton");
		const pasteSignatureButton = document.getElementById("pasteSignatureButton");

		const displaySelector = hasEthereum ? '.metamask-sign' : '.manual-sign';
		document.querySelectorAll(displaySelector).forEach(element => {
			element.style.display = 'block';
		});

		copyPayloadButton.addEventListener(
			"click",
			async (event) => {
				event.preventDefault();

				const payload = payloadInput.value;
				await navigator.clipboard.writeText(payload);
				alert('Payload copied to clipboard.');
			}
		);

		pasteSignatureButton.addEventListener(
			"click",
			async (event) => {
				event.preventDefault();
				const clipboardContents = await navigator.clipboard.readText();
				signatureInput.value = clipboardContents;
			}
		);

		signPayloadButton.addEventListener(
			"click",
			async (event) => {
				event.preventDefault();

				const payload = payloadInput.value;
				const account = accountInput.value;

				try {
					const signature = await ethereum.request({
						method: 'personal_sign',
						params: [payload, account]
					});

					signatureInput.value = signature;
				}
				catch (error) {
					console.error('Signing failed', error);

					if (error.code === 4001)
						// User rejected the request
						alert('Signature request was rejected by the user.');
					else
						alert('An error occurred during the signing process.');
				}
				signInButton.form.submit();
			}
		);
	}
}

function fixUpMorrisMetamaskSignPayloadRedirectForm() {
	const form = document.getElementById(morrisMetamaskSignPayloadRedirectFormId);
	if (form) {
		form.submit();
	}
}

fixUpMorrisMetamaskSelectWalletUI();
fixUpMorrisMetamaskSignPayloadUI();
fixUpMorrisMetamaskSignPayloadRedirectForm();

new MutationObserver((mutationsList) => {
	for (const mutation of mutationsList) {
		if (mutation.type === 'childList' && mutation.addedNodes.length > 0) {
			for (const node of mutation.addedNodes) {
				if (node.nodeType === 1) { // Check if the node is an element
					if (node.id === morrisMetamaskSelectWalletId) {
						fixUpMorrisMetamaskSelectWalletUI();
					}
					else if (node.id === morrisMetamaskSignPayloadId) {
						fixUpMorrisMetamaskSignPayloadUI();
					}
					else if (node.id === morrisMetamaskSignPayloadRedirectFormId) {
						fixUpMorrisMetamaskSignPayloadRedirectForm();
					}
				}
			}
		}
	}
})
	.observe(document.body, {
		childList: true,
		subtree: true
	});