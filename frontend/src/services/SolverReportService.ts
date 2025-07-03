import type { SolveRunDto } from "../dtos/solve-run-dto";

export default class SolverReportService {
	static async downloadFullJsonReport(solveRun: SolveRunDto): Promise<void> {
		this.download(JSON.stringify(solveRun), "application/json", "json");
	}

	static async downloadTable(solveRun: SolveRunDto): Promise<void> {
		let csv = '"Projects","Students"\n';
		solveRun.projects.forEach((allocation) => {
			csv += `"${allocation.project.name}","${allocation.students.map(s=>`${s.name} (${s.email})`).join(', ')}"\n`;
		});
		this.download(csv, "text/csv", "csv");
	}

	static async downloadFullCsvReport(solveRun: SolveRunDto): Promise<void> {
		let csv = '"StudentName","StudentEmail","ProjectName","ProjectClient"\n';
		solveRun.projects.forEach((allocation) => {
			allocation.students.forEach((student) => {
				csv += `"${student.name}","${student.email}","${allocation.project.name}","${allocation.project.client}"\n`;
			});
		});
		this.download(csv, "text/csv", "csv");
	}

	static download(data: string, type: string, ext: string) {
		const blob = new Blob([data], { type });
		const url = window.URL.createObjectURL(blob);
		const a = document.createElement("a");
		a.href = url;
		a.download = `report-${Date.now()}.${ext}`;
		document.body.appendChild(a);
		a.click();
		document.body.removeChild(a);
		window.URL.revokeObjectURL(url);
	}
}
