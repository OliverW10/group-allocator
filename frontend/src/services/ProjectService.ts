import type { ProjectDto } from "../dtos/project-dto";
import ApiService from "./ApiService";

export default class ProjectService {
    static getProjects() : Promise<ProjectDto[]> {
        return ApiService.get<ProjectDto[]>("/projects/get");
    }

    static getProjectById(projectId: string) : Promise<ProjectDto> {
        return ApiService.get<ProjectDto>(`/projects/get/${projectId}`);
    }
}