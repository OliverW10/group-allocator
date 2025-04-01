import type { ProjectDto } from "../dtos/project-dto";
import ApiService from "./ApiService";

export default class ProjectService {
    static getProjects() : Promise<ProjectDto[]> {
        return ApiService.get<ProjectDto[]>("/projects/get");
    }
}