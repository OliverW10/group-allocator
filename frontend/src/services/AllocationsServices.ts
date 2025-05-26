import type { PartialAllocation } from "../model/PartialAllocation"

export const removeAutoAllocated = (allocations: PartialAllocation[]) => {
	for (const allocation of allocations) {
        if (!allocation.manuallyAllocatedProject) {
            allocation.project = null
        }
        allocation.students = allocation.students.filter(x => x.manuallyAllocated)
    }
}
