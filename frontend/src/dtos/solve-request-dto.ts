/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { AllocationDto } from "./allocation-dto";
import { ClientLimitsDto } from "./client-limits-dto";

export class SolveRequestDto {
    preAllocations: AllocationDto[];
    clientLimits: ClientLimitsDto[];
    preferenceExponent: number;
    classId: number;
}
