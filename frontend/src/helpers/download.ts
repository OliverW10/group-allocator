import ApiService from "../services/ApiService";

export function downloadData(data: string, type: string, ext: string, filenamePrefix: string = 'report') {
    const blob = new Blob([data], { type });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `${filenamePrefix}-${Date.now()}.${ext}`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
}

export async function downloadFromUrl(endpoint: string, filename: string) {
	const url = await ApiService.makeUrl(endpoint)
	const a = document.createElement('a')
	a.href = url.toString()
	a.download = filename
	document.body.appendChild(a)
	a.click()
	document.body.removeChild(a)
}