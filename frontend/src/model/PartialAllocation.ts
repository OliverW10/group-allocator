import type { ProjectDto } from "../dtos/project-dto";
import type { StudentInfoDto } from "../dtos/student-info-dto";

export interface AllocatedStudentInfo extends StudentInfoDto {
    manuallyAllocated: boolean,
}

export interface PartialAllocation {
    project: ProjectDto | null,
    manuallyAllocatedProject: boolean,
    students: AllocatedStudentInfo[],
}

export function removeAutoAllocated(allocations: PartialAllocation[] | undefined) {
	for (const allocation of allocations ?? []) {
        if (!allocation.manuallyAllocatedProject) {
            allocation.project = null
        }
        allocation.students = allocation.students.filter(x => x.manuallyAllocated)
    }
}
