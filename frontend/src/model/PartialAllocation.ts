import type { ProjectDto } from "../dtos/project-dto";

export interface AllocatedStudentInfo extends StudentInfoDto {
    manuallyAllocated: boolean,
}

export interface PartialAllocation {
    project: ProjectDto | null,
    manuallyAllocatedProject: boolean,
    students: AllocatedStudentInfo[],
}