import type { ProjectDto } from "../dtos/project-dto";
import ApiService from "./ApiService";

export default class ProjectService {
    static getProjects() : Promise<ProjectDto[]> {
        return ApiService.get<ProjectDto[]>("/projects");
    }

    static deleteProject(projectId: string) : Promise<ProjectDto[]> {
        return ApiService.delete<ProjectDto[]>(`/projects/delete/${projectId}`);
    }
}
