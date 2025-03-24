import type GroupAllocationOption from "./GroupAllocationOption"

export default interface StudentPreferencesFormDto {
    firstName: string
    lastName: string
    studentNumber: string
    email: string
    hasProvidedConsentForm: boolean
    preferences: GroupAllocationOption[]
}