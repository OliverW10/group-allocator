import type { StudentSubmissionDto } from "../dtos/student-submission-dto";
import ApiService from "./ApiService";

export default class StudentsService {
    static getStudents(): Promise<StudentSubmissionDto[]> {
        return ApiService.get<StudentSubmissionDto[]>("/students");
    }
}