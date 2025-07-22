import type { SolveRunDto } from "../dtos/solve-run-dto";
import { downloadData } from "../helpers/download";

export default class SolverReportService {
	static async downloadFullJsonReport(solveRun: SolveRunDto): Promise<void> {
		downloadData(JSON.stringify(solveRun), "application/json", "json");
	}

	static async downloadTable(solveRun: SolveRunDto): Promise<void> {
		let csv = '"Projects","Students"\n';
		solveRun.projects.forEach((allocation) => {
			csv += `"${allocation.project.name}","${allocation.students.map(s=>`${s.name} (${s.email})`).join(', ')}"\n`;
		});
		downloadData(csv, "text/csv", "csv");
	}

	static async downloadFullCsvReport(solveRun: SolveRunDto): Promise<void> {
		let csv = '"StudentName","StudentEmail","ProjectName","ProjectClient"\n';
		solveRun.projects.forEach((allocation) => {
			allocation.students.forEach((student) => {
				csv += `"${student.name}","${student.email}","${allocation.project.name}","${allocation.project.client}"\n`;
			});
		});
		downloadData(csv, "text/csv", "csv");
	}
}
