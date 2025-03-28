import type GroupAllocationOption from "./GroupAllocationOption"

//must be moved to use backed dtos

export default interface StudentPreferencesFormDto {
    firstName: string
    lastName: string
    studentNumber: string
    email: string
    hasProvidedConsentForm: boolean
    preferences: GroupAllocationOption[]
}